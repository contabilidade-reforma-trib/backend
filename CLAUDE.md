# Praxis — Backend

## As regras deste repositório estão em [AGENTS.md](AGENTS.md)

Primeira vez aqui? [COMECE-AQUI.md](COMECE-AQUI.md) tem instalação, hooks e o prompt de abertura.

**Leia [AGENTS.md](AGENTS.md) inteiro agora, antes de escrever a primeira linha.** Ele é o arquivo canônico e vale para qualquer ferramenta de IA. As regras não estão duplicadas aqui de propósito — duas cópias divergem.

Depois dele, leia [docs/em-andamento.md](docs/em-andamento.md): é onde está a feature em curso e em que passo ela parou.

---

## Portões que valem mesmo que você não abra mais nada

Se por algum motivo você não puder ler o `AGENTS.md`, estes cinco continuam valendo:

1. **Teste é parte da entrega.** Toda alteração entra com teste de unidade; tocou banco, storage ou serviço externo, entra também teste de integração em schema isolado no Neon. Bug começa pelo teste que reproduz a falha.
2. **Peça revisão explícita** antes de mexer em regra de negócio, modelo de dados, contrato público entre módulos, regra de acesso ou prompt do copiloto. Pare, descreva, aguarde resposta.
3. **Antes de commitar:** faça a revisão de [docs/revisao-pre-commit.md](docs/revisao-pre-commit.md) sobre `git diff --staged` e **relate os achados à pessoa**. Achado bloqueante trava o commit.
4. **Antes de dar push:** a suíte inteira verde — não só a da sua feature. O hook `.githooks/pre-push` também barra, mas não conte com ele: avise a pessoa se algo alheio quebrou, em vez de contornar.
5. **Segredo nunca entra no repositório.** `appsettings.Development.json` é local e ignorado.

E o de sempre: não faça commit nem push sem o usuário pedir.

---

## Mapa dos documentos

| Arquivo | Para quê |
|---|---|
| [AGENTS.md](AGENTS.md) | **Regras canônicas.** Comece aqui |
| [docs/em-andamento.md](docs/em-andamento.md) | Feature em curso e em que passo parou |
| [docs/backlog.md](docs/backlog.md) | O que fazer, por prioridade |
| [docs/produto.md](docs/produto.md) | O que é o produto, para quem, como é vendido |
| [docs/arquitetura.md](docs/arquitetura.md) | Módulos, modelo de dados, esteira de ingestão, RAG |
| [docs/regras-desenvolvimento.md](docs/regras-desenvolvimento.md) | Detalhe de camadas, testes, EF/Dapper, git |
| [docs/glossario.md](docs/glossario.md) | Termos do domínio tributário — escreva-os exatamente assim |
| [docs/decisoes.md](docs/decisoes.md) | O que já foi decidido e o que está aberto |
| [docs/estado-atual.md](docs/estado-atual.md) | Onde o trabalho parou |
