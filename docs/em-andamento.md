# Em andamento — backend

**Leia este arquivo no começo de toda sessão.** É onde está a feature em curso e em que passo ela parou.

Regra: **uma feature por vez.** Se precisar começar outra antes de terminar esta, mova a atual para "Pausadas" com o motivo, em vez de deixar duas meio-feitas.

Marcação: `[x]` feito · `[>]` em execução agora · `[ ]` ainda não · `[!]` travado

---

## Feature atual

**Nenhuma.** A fundação e as entidades básicas ficaram prontas em 2026-09-05.

Próxima sugerida: **autenticação e os primeiros endpoints de negócio** (cadastro de organização + compra simulada). Sem autenticação, nenhum endpoint pode verificar direito de uso, e a regra "acesso é verificado a cada requisição" fica no papel.

---

## Concluídas recentemente

### Reestruturação e RAG — 2026-09-05

- Um projeto por camada de cada módulo; DbContext por módulo (D-19)
- Código todo em inglês (D-15), substituindo a D-04
- Domínio reduzido a User enquanto o negócio não está claro (D-16)
- Estrutura de RAG com pgvector, ingestão e busca (D-17)
- Ids gerados pelo domínio, ValueGeneratedNever em toda chave (D-18)
- Endpoints de vídeo no R2: URL assinada de envio e de reprodução
- 24 testes verdes; RAG exercitado ponta a ponta pela API
