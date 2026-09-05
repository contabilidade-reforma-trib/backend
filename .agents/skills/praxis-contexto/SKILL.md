---
name: praxis-contexto
description: Carrega o contexto e as regras obrigatórias do projeto Praxis (copiloto fiscal e mentoria em reforma tributária). Use no início de toda sessão, antes de escrever qualquer código, e novamente ao trocar de repositório.
---

# Contexto do projeto Praxis

## Onde você está

Esta pasta contém **dois repositórios git independentes**:

- `frontend/` — Next.js, deploy na Vercel
- `backend/` — .NET, deploy no Railway

A pasta que os contém **não é um repositório**. Consequências, todas obrigatórias:

- **Nunca rode `git init` na pasta raiz.** Isso aninharia os dois repositórios e quebraria os deploys.
- Todo comando git roda **dentro** de um dos dois: `git -C frontend status`, `git -C backend log`.
- **Commit e push são sempre por repositório.** Uma alteração que atravessa os dois vira dois commits, um em cada, cada um passando pelos seus próprios portões.
- Se um comando git falhar dizendo que não encontrou repositório, a causa é você estar na raiz. Entre no repositório certo e repita — não tente criar um repositório.

## As regras não estão aqui

**De propósito.** As regras moram dentro dos repositórios, são versionadas e mudam junto com o projeto. Este arquivo só te manda lê-las, para nunca existir uma segunda cópia divergindo em silêncio.

**Leia agora, inteiros, antes de tocar em qualquer arquivo:**

| Vai mexer em | Leia |
|---|---|
| `frontend/**` | `frontend/AGENTS.md` e `frontend/docs/em-andamento.md` |
| `backend/**` | `backend/AGENTS.md` e `backend/docs/em-andamento.md` |

Vai mexer nos dois nesta sessão? Leia os quatro.

O `AGENTS.md` traz as regras do repositório. O `docs/em-andamento.md` traz a feature em curso e em que passo ela parou — é de onde você continua o trabalho, em vez de recomeçar.

## Leia também os critérios de qualidade, agora

Leia, do mesmo repositório, **`docs/revisao-pre-commit.md`**. Ele descreve os eixos com que todo diff é avaliado antes de virar commit: separação de responsabilidade, centralização do que se repete, simplificação (KISS), legibilidade e nomenclatura, e aderência às regras do `AGENTS.md`.

Carregue esses critérios **no início da sessão**, não só na hora do commit. Eles não são um checklist de conferência no fim — são o padrão com que o código deve ser escrito desde a primeira linha. Código que você teria de corrigir na revisão não deveria ter sido escrito assim.

Na prática, enquanto escreve, pergunte-se:

- Isto está na camada certa, ou estou pondo regra onde ela não mora?
- Já existe algo no repositório que faz isso? Estou duplicando?
- Existe versão mais simples que resolve o mesmo problema?
- O nome diz exatamente o que a coisa faz, sem enganar?

## Confirme antes de começar

Depois de ler, **liste de volta para a pessoa**:

1. A regra de teste.
2. O que exige revisão explícita dela antes de você mexer.
3. O portão de commit e o portão de push.
4. Os eixos de qualidade que vão ser cobrados no diff.

Não é formalidade: se você não conseguir listar, não leu, e não deve começar.

## Sempre

- **Não faça commit nem push sem a pessoa pedir.**
- Quando ela pedir commit: faça antes a revisão do diff preparado descrita em `docs/revisao-pre-commit.md` do repositório, e **relate os achados a ela** antes de commitar.
- Quando ela pedir push: a suíte **inteira** do repositório precisa estar verde, não só os testes da sua feature.
- Ao terminar uma etapa, atualize o `docs/em-andamento.md` do repositório. É o que a próxima sessão vai ler.

## Ao trocar de repositório no meio da sessão

Estava no front e vai mexer no back, ou o contrário: **leia o `AGENTS.md` e o `docs/em-andamento.md` do repositório novo antes de continuar.** As regras dos dois não são iguais.
