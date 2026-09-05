# Revisão pré-commit — backend

## Quando

Antes de **todo** `git commit` feito por IA. A revisão é sobre o que está preparado, não sobre o repositório inteiro.

```bash
git diff --staged
```

Se nada estiver preparado, revise o que será preparado e diga isso no relatório.

## O que olhar aqui

Passe pelos cinco eixos abaixo. Para cada achado registre **arquivo:linha**, o que está errado, e a correção concreta. Achado sem localização e sem proposta de correção não serve.

### 1. Separação de responsabilidade

- Regra de negócio dentro de endpoint, controller ou `Infrastructure`? Ela pertence a `Domain` ou ao caso de uso.
- `Domain` com atributo de EF, de serialização ou referência a framework?
- `Api` alcançando `Domain` ou `Infrastructure` direto, pulando `Application`?
- **Entidade de domínio atravessando fronteira de módulo?** Entre módulos só passa DTO, por `QueryService` ou caso de uso exposto em `Application`.
- Módulo lendo tabela, `DbContext` ou entidade de outro módulo? É bloqueante.
- Caso de uso fazendo trabalho de repositório, ou repositório decidindo regra?

### 2. Centralização

- A mesma regra aparece em dois lugares? Uma delas vai ficar para trás na próxima mudança.
- Validação, mapeamento ou formatação copiada entre arquivos?
- Literal repetida (nome de política, prefixo de tabela, limite numérico) que deveria ser constante nomeada?
- Já existe no repositório algo que faz isso? Procure antes de aceitar código novo — duplicar utilitário é dos achados mais comuns.

Cuidado com o excesso: duas ocorrências parecidas nem sempre são a mesma regra. Centralizar coisas que só coincidem hoje cria acoplamento pior que a duplicação.

### 3. Simplificação — KISS

- Abstração com **uma** implementação e nenhuma segunda à vista.
- Camada de indireção que só repassa chamada.
- Generalização para caso que ninguém pediu.
- `try/catch` que captura e relança sem acrescentar nada.
- Método longo demais ou com mais de três níveis de indentação.
- Parâmetro booleano que controla fluxo — quase sempre são dois métodos.
- Configuração para algo que nunca vai variar.

A pergunta guia: **existe uma versão mais simples disto que resolve o mesmo problema?** Se existir, é achado.

### 4. Legibilidade e nomenclatura

- Nome genérico: `data`, `item`, `obj`, `result2`, `Helper`, `Manager`, `Util`, `Service` solto.
- Abreviação inventada.
- Idioma misturado no mesmo identificador. Domínio em português, técnico em inglês, nunca no mesmo nome.
- Boolean sem `Esta`, `Possui`, `Deve`, `Pode`.
- **Nome que engana** — o método faz mais do que o nome diz, ou faz outra coisa. É o achado mais grave desta seção.
- Comentário explicando *o quê* em vez de *por quê*: normalmente indica que falta um nome melhor.

### 5. Regras do repositório

Consulte [AGENTS.md](../AGENTS.md) e [docs/regras-desenvolvimento.md](./regras-desenvolvimento.md).

- **Teste de unidade para o que mudou?** Faltou, é bloqueante.
- Tocou banco, storage ou serviço externo sem teste de integração?
- Segredo no diff? `appsettings.Development.json` preparado? Bloqueante, sem discussão.
- **Dapper com nome de coluna em string crua**, sem `nameof`? Bloqueante.
- Dapper usado onde o EF Core resolveria? EF é o padrão; Dapper precisa de motivo escrito na consulta.
- IO sem `async` ou sem `CancellationToken` propagado?
- Consulta dentro de laço, listagem sem paginação?
- Migration destrutiva sem revisão prévia do usuário?
- Alteração em regra de negócio, contrato público, regra de acesso ou prompt **sem** ter pedido revisão explícita antes?

## Bloqueiam neste repositório

Além dos bloqueantes comuns (segredo, teste ausente, regra duplicada, nome que engana):

- Módulo lendo tabela, entidade ou `DbContext` de outro módulo.
- Entidade de domínio atravessando fronteira de módulo.
- Dapper com nome de coluna em string crua, sem `nameof`.
- Regra de negócio dentro de endpoint ou de `Infrastructure`.
- Migration destrutiva sem revisão prévia.

## Severidade

| Nível | Significado |
|---|---|
| **Bloqueia** | Não commite. Corrija, ou peça dispensa explícita à pessoa. |
| **Recomenda** | Vale corrigir agora; a pessoa decide. |
| **Observa** | Fica registrado, não trava nada. |

Bloqueiam nos dois repositórios: **segredo no diff**, **teste ausente** para o que mudou, **regra duplicada ou no lugar errado**, **nome que engana**, e alteração de núcleo feita **sem a revisão explícita** que o `AGENTS.md` exige.

## Formato do relatório

Sempre relate à pessoa antes de commitar, mesmo quando não houver nada.

```
REVISÃO PRÉ-COMMIT — 7 arquivos, +212/−38

BLOQUEIA
  caminho/do/arquivo.ext:64
    O que está errado, em uma frase.
    → A correção concreta.

RECOMENDA
  caminho/do/arquivo.ext:22
    O que está errado.
    → A correção concreta.

OBSERVA
  caminho/do/arquivo.ext:31
    Observação que não trava nada.

Nada bloqueante além do item acima. Confirma o commit?
```

Achado sem **arquivo:linha** e sem proposta de correção não serve.

## Regras da própria revisão

- **Não corrija silenciosamente.** Relate. A pessoa decide o que entra.
- Não commite por cima de achado bloqueante sem dispensa explícita.
- Não invente achado para parecer útil. **"Nada a apontar" é resultado legítimo** e deve ser dito com todas as letras.
- Revise **o diff**, não o repositório. Código antigo encontrado de passagem vira item no `docs/backlog.md` do repositório, não bloqueio de commit.
- Alteração que atravessa os dois repositórios gera **dois commits**, cada um com sua própria revisão.
