# Regras de desenvolvimento — backend

Complementa as regras inegociáveis do [AGENTS.md](../AGENTS.md). Aqui está o detalhe de como cumprir cada uma.

## 1. Definição de pronto

Uma tarefa só está pronta quando **todos** os itens abaixo estão verdadeiros:

- [ ] `dotnet build` sem warning novo
- [ ] `dotnet test` verde
- [ ] Teste de unidade cobrindo o comportamento que mudou
- [ ] Teste de integração, se tocou banco, storage ou serviço externo
- [ ] Nomes legíveis, no idioma certo (domínio em português, técnico em inglês)
- [ ] Nenhum segredo no diff
- [ ] `docs/em-andamento.md` com os passos atualizados
- [ ] `docs/estado-atual.md` atualizado
- [ ] `docs/decisoes.md` atualizado, se houve decisão
- [ ] `docs/backlog.md` atualizado, se algo novo surgiu ou foi concluído

Relatar "pronto" com qualquer item falso é erro grave. Se algo ficou de fora, diga o que ficou e por quê.

## 2. Testes

### 2.1 Unidade — sempre

Vive em `tests/Praxis.UnitTests`, espelhando a pasta do módulo:

```
tests/Praxis.UnitTests/
  Copiloto/Application/ResponderConsultaTests.cs
  Mentoria/Domain/TrilhaTests.cs
```

- Um arquivo de teste por classe testada, mesmo nome + `Tests`.
- Nome do teste descreve comportamento e resultado, em português:
  `Deve_recusar_resposta_quando_nenhuma_fonte_atinge_o_score_minimo`
- Sem banco, sem rede, sem relógio real. Tempo entra por abstração (`IRelogio`), nunca `DateTime.Now`.
- Testa regra de negócio e caso de borda, não getter/setter.
- **Bug corrigido começa pelo teste que reproduz o bug falhando.** Escreva o teste, veja falhar, corrija, veja passar.

### 2.2 Integração — sempre que houver IO real

Vive em `tests/Praxis.IntegrationTests`. Aponta para o **mesmo banco Neon**, mas em **schema isolado por execução**, nunca no `public`.

Contrato obrigatório:

1. No início da execução, cria schema `teste_<guid-sem-hifen>`.
2. Aplica as migrations nesse schema.
3. Roda os testes com a connection string apontando `Search Path` para ele.
4. **No fim, derruba o schema com `DROP SCHEMA ... CASCADE`.**
5. A limpeza roda em `DisposeAsync` / `finally`, de forma que **teste que falha ou estoura no meio ainda derruba o schema**.
6. Ao subir a fixture, derruba schemas `teste_%` com mais de 24h — rede de segurança para execução interrompida por queda de máquina.

O esqueleto vive em `tests/Praxis.IntegrationTests/Infra/BancoDeTesteFixture.cs`. Se ele ainda não existir, criá-lo é a primeira tarefa antes do primeiro teste de integração.

Configuração em `appsettings.json`:

```json
"TestesIntegracao": { "PrefixoSchema": "teste_", "DerrubarSchemaAoFinal": true }
```

Nunca desligue `DerrubarSchemaAoFinal` no repositório. Se precisar inspecionar dados, desligue localmente e reverta.

### 2.3 O que não testar

Não escreva teste que só confirma o framework (que o EF salva, que o ASP.NET roteia). Teste a **sua** regra.

## 3. Camadas

| Camada | Pode referenciar | Nunca referencia |
|---|---|---|
| `Domain` | nada além do próprio módulo e tipos base | `Application`, `Infrastructure`, EF, ASP.NET |
| `Application` | `Domain`, abstrações de `Shared` | implementação concreta de infra |
| `Infrastructure` | `Application`, `Domain` | outro módulo |
| `Api` | `Application` | `Domain` diretamente, `Infrastructure` |

Regras práticas:

- Entidade de domínio não tem atributo de EF nem de serialização. Mapeamento fica em `Infrastructure`.
- `Application` define a interface do repositório; `Infrastructure` implementa.
- `Api` traduz HTTP para caso de uso e volta. Não contém `if` de regra de negócio.
- Erro esperado volta como resultado (`Result`), não como exceção. Exceção é para o inesperado.
- Resposta de erro da API usa `ProblemDetails`, com mensagem que diz o que houve e o que fazer.

## 4. Fronteira entre módulos

**A única porta entre módulos é a camada `Application`.**

- O módulo dono expõe em `Application` um **contrato público**: `QueryService` para leitura, caso de uso para escrita. A troca é sempre por **DTO**.
- O consumidor injeta a interface e recebe DTO. Não referencia `Domain` nem `Infrastructure` do outro módulo.
- **Entidade de domínio nunca atravessa a fronteira.** Se ela vazar, o outro módulo passa a depender de regra que não é dele e a fronteira morre em silêncio.
- Nada de `JOIN`, view ou `DbContext` cruzando módulo.
- Cada módulo é dono das suas tabelas. Prefixo por módulo: `mentoria_trilha`, `copiloto_consulta`.
- Comunicação assíncrona, quando couber, por evento de domínio — também com carga em DTO.

Exemplo obrigatório: `Copiloto` precisa saber se a organização pode consultar.

```csharp
// Assinaturas/Application/IConsultaDeDireitoDeUso.cs — contrato público
public interface IConsultaDeDireitoDeUso
{
    Task<DireitoDeUsoDto?> ObterAtivo(
        Guid organizacaoId, Produto produto, CancellationToken cancellationToken);
}
```

`Copiloto` injeta essa interface e segue. Ele não conhece a tabela, não conhece a entidade, não conhece a regra de vigência, e **não guarda cópia da resposta**.

## 5. Banco e migrations

### 5.1 EF Core é o padrão

**EF Core primeiro, sempre.** Escrita, leitura, migration, projeção. É ele que garante que renomear uma propriedade quebre a compilação em vez de quebrar produção.

**Dapper é exceção.** Só entra quando o EF tornaria a consulta inviável ou absurdamente ineficiente — agregação com window function, CTE recursiva, consulta vetorial crua. Quando entrar:

1. O **motivo vai escrito em comentário na própria consulta**. Sem motivo, o achado bloqueia o commit.
2. Os nomes de campo saem de **`nameof`**, nunca de string crua:

```csharp
// Dapper: ranking por similaridade com window function, que o EF não traduz.
var sql = $"""
    SELECT t.{nameof(Trecho.Id)}, t.{nameof(Trecho.Conteudo)},
           ROW_NUMBER() OVER (PARTITION BY t.{nameof(Trecho.DocumentoId)}
                              ORDER BY t.Embedding <=> @consulta) AS posicao
    FROM copiloto_trecho t
    WHERE t.{nameof(Trecho.VigenciaFim)} IS NULL OR t.{nameof(Trecho.VigenciaFim)} >= @hoje
    """;
```

A razão é uma só: **refatoração futura tem que quebrar a consulta em tempo de compilação.** Nome de propriedade dentro de string sobrevive ao rename e explode meses depois, em produção, num caminho que ninguém testou.

Quando o nome da coluna no banco diferir do nome da propriedade, mapeie explicitamente — e ainda assim ancore no `nameof` da propriedade, com o apelido no SQL.

### 5.2 Migrations

- Migration é versionada, tem nome descritivo e é revisada como código.
- Migration destrutiva (drop de coluna/tabela, mudança de tipo com perda) exige **pedido de revisão explícito** antes de ser escrita.
- Toda tabela tem `CriadoEm` e `AtualizadoEm` em UTC.
- Nada de `SELECT *` implícito em consulta de listagem; projete só o que a tela usa.
- pgvector vive no mesmo banco. Índice vetorial é criado por migration, não à mão.

## 6. Chamada de IA

- Todo acesso a LLM passa por uma abstração em `Praxis.Shared` (`IProvedorDeIa`). Nenhum módulo chama SDK de fornecedor diretamente. O produto vai trocar de provedor.
- Toda consulta grava: modelo usado, tokens de entrada, tokens de saída, custo estimado, usuário e organização.
- Prompt fica em arquivo versionado, não embutido em string no meio do código.
- Mudança de prompt ou de estratégia de recuperação exige **pedido de revisão explícito** — muda a resposta que o contador leva para o cliente dele.

## 7. Estilo

- C# moderno: `record` para dado imutável, `required`, pattern matching, `nullable` habilitado e respeitado.
- Um arquivo por tipo público.
- Sem região (`#region`).
- Comentário explica **por quê**, nunca **o quê**. Código que precisa de comentário para dizer o que faz precisa de nome melhor.
- Método que passa de ~30 linhas ou de 3 níveis de indentação provavelmente é dois métodos.

## 8. Git

- Não commitar nem dar push sem o usuário pedir.
- Branch por tarefa: `feat/`, `fix/`, `chore/` + descrição curta em kebab-case.
- Mensagem de commit no imperativo, em português, dizendo o efeito: `Adiciona verificação de direito de uso na consulta ao copiloto`.
- `appsettings.Development.json` nunca entra. Confirme com `git status` antes de qualquer commit.

### 8.1 Portão de commit

Commit feito por IA passa antes pela skill **`revisao-pre-commit`**, sobre `git diff --staged`. Os achados são **relatados à pessoa** antes do commit. Achado bloqueante (segredo, teste ausente, fronteira de módulo furada, regra duplicada, Dapper com string crua, nome que engana) trava o commit até ser corrigido ou dispensado explicitamente.

### 8.2 Portão de push

Push feito por IA exige a **suíte inteira verde** — unidade e integração, do repositório todo, não só o que a feature tocou:

```bash
dotnet test
```

Um único teste vermelho, mesmo em área que você não encostou, cancela o push. Nesse caso, **avise a pessoa e pergunte o que fazer**. Não marque como ignorado, não comente o teste, não deixe para depois.
