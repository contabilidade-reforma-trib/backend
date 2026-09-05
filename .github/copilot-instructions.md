# Instruções do repositório

As regras canônicas deste repositório estão em **[AGENTS.md](../AGENTS.md)**, na raiz.

**Abra e leia o `AGENTS.md` inteiro antes de propor ou escrever qualquer código.** Ele não está duplicado aqui de propósito: duas cópias divergem.

Depois dele, leia `docs/em-andamento.md` — é onde está a feature em curso e em que passo ela parou.

Resumo mínimo, caso você não consiga abrir o arquivo:

- Toda alteração entra com **teste**. Bug começa pelo teste que reproduz a falha.
- Antes de mexer em regra de negócio, modelo de dados, contrato público, regra de acesso ou tokens de design: **pare, descreva e peça revisão explícita**.
- Antes de commitar: revisão do diff (separação de responsabilidade, centralização, KISS, legibilidade, nomenclatura) **relatada à pessoa**.
- Antes de dar push: **a suíte inteira** verde, não só a da sua feature.
- Nomes dizem o que a coisa faz. Domínio em português, técnico em inglês.
- Segredo nunca entra no repositório.
- Não commitar nem dar push sem a pessoa pedir.
