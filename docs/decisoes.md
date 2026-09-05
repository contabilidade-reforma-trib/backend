# Decisões

Registro curto de decisão. Serve para não relitigar o que já foi resolvido, e para deixar explícito o que ainda está aberto.

Formato: **contexto → decisão → consequência**. Quando uma decisão for revertida, marque como substituída e escreva a nova abaixo, sem apagar a antiga.

---

## Decididas

### D-01 · A conta é a organização, não a pessoa
O contador compra para si, mas pode colocar funcionários do escritório para usar.
**Decisão:** a entidade de cobrança e de acesso é `Organizacao`. Todo usuário pertence a uma, inclusive o autônomo (organização de um usuário só).
**Consequência:** evita a migração cara de converter conta pessoal em conta de empresa. Histórico de consulta e progresso de aula ficam no `Usuario`, não na organização.

### D-02 · Copiloto e Mentoria são produtos independentes
**Decisão:** vendidos separados, juntos, ou um depois do outro. Nenhum depende do outro para funcionar. A Mentoria ensina o assunto, não o uso do copiloto.
**Consequência:** acesso modelado como `DireitoDeUso` por produto, com vigência própria. `Assinaturas` é a única autoridade; os outros módulos perguntam.

### D-03 · Monolito modular com fronteira real — parcialmente SUBSTITUÍDA pela D-19
**Decisão:** um processo, pastas por contexto, `Domain/Application/Infrastructure` dentro de cada. Módulo não lê tabela de outro.
**Consequência:** extrair um módulo depois não exige reescrever domínio. Custo hoje: disciplina manual, já que a fronteira não é imposta pelo compilador enquanto forem pastas no mesmo projeto. Se a disciplina falhar, promover cada contexto a projeto próprio.

### D-04 · Domínio em português, técnico em inglês — SUBSTITUÍDA pela D-15
**Decisão:** `Trilha`, `Aula`, `SaldoCredor`, `Aliquota` em português; `Repository`, `Handler`, `CancellationToken` em inglês. Nunca misturado no mesmo identificador.
**Consequência:** o vocabulário do código bate com o vocabulário das mentoras, que são a fonte da regra.

### D-05 · Sem fonte, sem resposta
**Decisão:** o copiloto não afirma nada que não esteja na base. Abaixo do score mínimo, responde que não sabe e manda a pergunta para a fila de revisão.
**Consequência:** é a regra de negócio central. Qualquer alteração nela exige revisão explícita.

### D-07 · Custo de IA medido desde a POC
**Decisão:** toda consulta grava modelo, tokens de entrada e saída, custo estimado, usuário e organização.
**Consequência:** dá para saber o teto real por trás da promessa de "consultas ilimitadas" antes de ela virar prejuízo.

### D-09 · Provedor de IA abstraído
**Decisão:** nenhum módulo chama SDK de fornecedor direto. Tudo passa por `IProvedorDeIa` em `Praxis.Shared`.
**Consequência:** trocar OpenAI por outro provedor é mudar uma implementação, não o produto.

### D-10 · Teste de integração em schema isolado
**Decisão:** mesmo banco Neon, schema `teste_<guid>` por execução, derrubado no fim, com limpeza garantida mesmo em falha e varredura de schemas órfãos com mais de 24h.
**Consequência:** não precisa de segundo banco na POC e não há risco de sujar o `public`.

### D-14 · O front fala com o backend por um BFF, não direto do navegador
Chamada direta do navegador para a API obrigava `NEXT_PUBLIC_API_URL` no bundle, CORS no backend, e — o que pesou de verdade — colocaria o token de sessão em lugar acessível por JavaScript quando a autenticação chegasse.
**Decisão:** o navegador só fala com a origem da Vercel. Route handlers em `src/app/api/` rodam no servidor do Next e repassam para o backend, usando `API_URL` (variável de servidor, fora do bundle).
**Consequência:** o backend deixa de precisar de CORS, e a lista vazia de origens passa a significar *nenhuma origem permitida* em vez de *todas*. O token de autenticação vai viver em cookie `httpOnly`, ilegível para o JavaScript da página. Custos aceitos: um salto a mais de rede, invocações de função na Vercel, e cuidado extra quando o streaming do copiloto atravessar o proxy.
**Tomada antes da autenticação de propósito** — depois dela, mudar exigiria reescrever o fluxo de login.

---

## Abertas

### D-06 · Certificado conta como EPC do CFC?
Emitir PDF com carga horária é trivial. Fazer valer como Educação Profissional Continuada exige credenciamento junto ao CFC.
**Impacto:** se valer, é argumento de venda forte e muda a copy da landing e o modelo de `Certificado`. **Aguardando definição das mentoras.**

### D-08 · Entrega de vídeo
R2 guarda o arquivo, mas não faz transcode nem entrega HLS com URL assinata pronta.
**Opções:** (a) Cloudflare Stream — resolve transcode, HLS e assinatura, custo por minuto armazenado; (b) R2 + transcode próprio — mais barato em escala, muito mais trabalho.
**Impacto:** muda `ChaveVideo` e a esteira de ingestão. **Aguardando decisão.**

### D-11 · Vigência dos documentos
A transição é escalonada até 2033. Sem vigência no trecho, o copiloto responde 2029 com regra de 2026.
**Situação:** o campo existe (`VigenciaInicio`, `VigenciaFim`, ambos opcionais e nulos por padrão = sempre vigente). **Quem preenche são as mentoras, ao subir cada documento.** Não trava desenvolvimento; trava qualidade da resposta.

### D-12 · Meio de pagamento real
Pix e boleto são obrigatórios para este público; cartão sozinho não atende.
**Impacto:** escolher gateway (Asaas, Pagar.me, Stripe) muda `Pagamento` e o fluxo de compra. Na POC é simulado. **Aguardando.**

### D-13 · Retenção de dado de terceiro
O contador vai colar CNPJ, faturamento e nome de clientes dele no chat.
**Impacto:** define prazo de retenção de `Consulta`, política de privacidade e cláusula de não uso para treino. **Aguardando definição jurídica** — a advogada do time é a pessoa certa.

### D-15 · Todo o código em inglês
Substitui a **D-04**, que definia domínio em português. O usuário decidiu inglês para domínio e técnico, e é a base de código dele.
**Decisão:** classe, método, variável, tabela, coluna e nome de teste em inglês. A exceção são termos legais brasileiros sem tradução honesta — `SimplesNacional`, `LucroPresumido`, `CNPJ` — que continuam como são e estão no glossário.
**Consequência:** o front usa os nomes do backend em tudo que espelha o contrato da API (`HealthResponse`, `database.connected`, `/api/health`). Componentes e textos de interface do front seguem em português; não há refatoração pendente deles.

### D-16 · Domínio reduzido a `User` enquanto o negócio não está claro
Havia `Organizacao`, `Assinatura`, `DireitoDeUso`, `Pagamento` e `PerfilDeUso` antes de o domínio estar entendido.
**Decisão:** manter apenas `User`, com dados cadastrais. Organização, assinatura e titularidade voltam quando o modelo de negócio estiver definido.
**Consequência:** a regra de titularidade que havíamos desenhado (usuário sem organização responde por si; com organização, a assinatura é da organização) está registrada aqui e não se perde — mas não vira tabela antes da hora. `IAccessRightQuery` era o lugar certo para ela.

### D-17 · Estrutura de RAG antes de existir chave de IA
**Decisão:** `KnowledgeDocument` + `DocumentChunk` com pgvector, `IngestDocument` e `SearchKnowledge`, e um `IAiProvider` com implementação determinística de reserva.
**Consequência:** a esteira inteira roda e é testada sem gastar um centavo, e trocar por um provedor real é uma linha no registro de serviços. O que a implementação de reserva **não** prova é que a recuperação acha a passagem certa — isso exige embeddings reais e o conjunto de perguntas-gabarito das mentoras.
**Vigência já modelada:** `ValidFrom`/`ValidUntil` no documento, filtrados na consulta. A reforma é escalonada até 2033; recuperar material vencido é o erro que só aparece quando o cliente é autuado.

### D-18 · Ids gerados pelo domínio, nunca pelo banco
Para chave `Guid`, o EF assume `ValueGeneratedOnAdd` e decide inserir ou atualizar conforme o Id seja default. Como as entidades geram o próprio Id, ele classificava entidade nova como existente e emitia `UPDATE` de zero linhas — a reindexação de um documento quebrava com `DbUpdateConcurrencyException`.
**Decisão:** `ValueGeneratedNever()` na chave de toda entidade.
**Consequência:** vale para qualquer entidade futura. Esquecer isso produz um erro que não aponta para a causa.

### D-19 · Um projeto por camada de cada módulo
Substitui a parte da **D-03** que dizia "pastas no mesmo projeto, fronteira mantida por disciplina". A disciplina não é verificável; a referência de projeto é.
**Decisão:** `src/Modules/<Módulo>/Praxis.<Módulo>.{Domain,Application,Infrastructure}`, cada um um projeto. E **um DbContext por módulo**, cada qual com sua tabela de histórico de migration, no mesmo banco.
**Consequência:** ler tabela de outro módulo deixa de ser proibido por convenção e passa a ser impossível — o contexto não conhece aquelas entidades, e a referência de projeto não existe. Custo: mais projetos, migrations por contexto, e transação que atravesse módulos passa a exigir decisão explícita (não é necessária hoje).
**Fronteira:** consumidor referencia apenas o `Application` do outro módulo. Querer referenciar `Domain` ou `Infrastructure` alheio é sinal de desenho errado.
