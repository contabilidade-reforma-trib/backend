# Nova feature no backend

Siga na ordem. Pular etapa é o que produz código que compila e regra que ninguém consegue auditar depois.

## 1. Antes de escrever qualquer coisa

Responda para si mesmo, e diga ao usuário se alguma resposta faltar:

- Qual **contexto** é dono disso? (`Identidade`, `Assinaturas`, `Copiloto`, `Mentoria`) Se parecer que são dois, provavelmente um deles precisa **perguntar** ao outro por contrato público.
- Isso toca **regra de negócio, modelo de dados, contrato público, regra de acesso ou prompt**? Se sim, **pare aqui**: descreva o que pretende fazer e peça revisão explícita. Só siga após resposta.
- Existe decisão em `docs/decisoes.md` que já resolve isso? Não relitigue o que está decidido.

## 2. Domínio primeiro

Escreva a entidade ou o objeto de valor em `<Contexto>/Domain`, com as invariantes dentro dele.

- Sem atributo de EF, sem atributo de serialização, sem referência a framework.
- Estado inválido deve ser impossível de construir. Validação no construtor ou em método fábrica, não espalhada em `if` no caso de uso.
- Nomes em português, legíveis: `PodeAcessarCopiloto()`, não `CheckAccess()`.

**Teste de unidade do domínio agora**, antes de seguir. Regra e caso de borda.

## 3. Caso de uso

Em `<Contexto>/Application`:

- Um caso de uso por arquivo, nome que diz o que faz: `ResponderConsulta`, `LiberarAcessoAposCompra`.
- Declare aqui a interface do repositório de que precisa. A implementação não é problema seu nesta etapa.
- Erro esperado volta como `Result`, não como exceção.
- `CancellationToken` entra e é propagado.

**Teste de unidade do caso de uso**, com repositório falso. Sem banco.

## 4. Infraestrutura

Em `<Contexto>/Infrastructure`: implementação do repositório, mapeamento EF, migration.

- Migration com nome descritivo.
- Migration destrutiva exige revisão explícita — volte à etapa 1.
- Tabela prefixada pelo contexto: `mentoria_aula`, `copiloto_trecho`.
- `CriadoEm` e `AtualizadoEm` em UTC.

**Teste de integração** com schema isolado — invoque a skill `teste-integracao`.

## 5. API

Em `src/Praxis.Api/Endpoints`:

- O endpoint traduz HTTP para caso de uso e volta. Nenhum `if` de regra de negócio aqui.
- Valida direito de uso perguntando ao módulo `Assinaturas`. Nunca deduza do plano nem guarde cópia.
- Erro sai como `ProblemDetails`, com mensagem que diz o que houve e o que fazer.

## 6. Fechamento

```bash
dotnet build
```

```bash
dotnet test
```

- [ ] Build sem warning novo
- [ ] Testes verdes
- [ ] Teste de unidade escrito
- [ ] Teste de integração escrito, se tocou IO
- [ ] `docs/estado-atual.md` atualizado
- [ ] `docs/decisoes.md` atualizado, se houve decisão
- [ ] `git status` sem `appsettings.Development.json`

Não commite nem faça push sem o usuário pedir.

## Correção de bug

Mesma trilha, com uma diferença no começo: **escreva primeiro o teste que reproduz o bug e veja-o falhar.** Só então corrija. Um bug que some sem teste vermelho antes não tem prova de que foi corrigido.
