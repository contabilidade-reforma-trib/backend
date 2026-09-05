# Backlog — backend

Tudo que deve ou pode ser feito, com prioridade. O que está **em execução agora** não fica aqui — fica em [em-andamento.md](em-andamento.md).

## Escala de prioridade

| Nível | Significado | Critério |
|---|---|---|
| **P0** | Trava outras coisas | Ninguém consegue avançar enquanto não existir |
| **P1** | Necessário para a POC | Sem isso a POC não demonstra o produto |
| **P2** | Importante, depois da POC | Necessário para produção, não para demonstrar |
| **P3** | Ideia | Vale considerar; ninguém está esperando |

Ao concluir um item, mova para **Concluídos** com a data. Não apague — o histórico explica decisões futuras.

---

## P0 — trava outras coisas

| # | Item | Observação |
|---|---|---|
| B-01 | `Praxis.Shared`: `Result`, `IRelogio`, `EntidadeBase` | Todo módulo depende disso |
| B-02 | `BancoDeTesteFixture` com schema isolado e derrubada garantida | Sem isso não existe teste de integração; ver skill `teste-integracao` |
| B-03 | `DbContext`, configuração do Npgsql e primeira migration | Destrava todos os módulos |

## P1 — necessário para a POC

| # | Item | Observação |
|---|---|---|
| B-04 | Módulo `Identidade`: `Organizacao`, `Usuario`, `PerfilDeUso` | Conta é a organização (D-01) |
| B-05 | Autenticação e emissão de token | Definir mecanismo antes de começar |
| B-06 | Módulo `Assinaturas`: `DireitoDeUso` + `IConsultaDeDireitoDeUso` | Contrato público que Copiloto e Mentoria consomem |
| B-07 | Compra simulada das três ofertas (Copiloto, Mentoria, Combo) | Pagamento real é P2 |
| B-08 | Módulo `Mentoria`: `Trilha`, `Modulo`, `Aula` + leitura pelo front | Maior superfície do sistema |
| B-09 | Cadastro que monta a trilha: gravar perfil e ordenar trilhas | Regra de ordenação precisa vir das mentoras |
| B-10 | `IProvedorDeIa` + implementação falsa para teste | Pode ser feito antes da chave existir |
| B-11 | Módulo `Copiloto`: `Documento`, `Trecho`, pgvector, ingestão | Depende de chave de IA |
| B-12 | Recuperação com filtro de vigência e score mínimo | "Sem fonte, sem resposta" (D-05) |
| B-13 | Registro de tokens e custo por consulta | D-07 |
| B-14 | Upload para R2 com URL assinada | Vídeo e material |
| B-15 | Conjunto de avaliação: 40 perguntas com fontes esperadas | Insumo vem das mentoras — pedir cedo, demora |
| B-16 | Seed de dados de exemplo versionado | 1 trilha, 3 aulas, 5 documentos; destrava trabalho sem depender de conteúdo real |
| B-17 | Área administrativa: CRUD de trilha/módulo/aula | Telas já desenhadas |

## P2 — depois da POC

| # | Item | Observação |
|---|---|---|
| B-18 | Gateway de pagamento real com Pix e boleto | D-12 em aberto |
| B-19 | Esteira de transcrição (Whisper) | Em espera por decisão do usuário |
| B-20 | Entrega de vídeo com HLS | D-08 em aberto |
| B-21 | Fila de revisão alimentada pelo joinha negativo | Fecha o ciclo de qualidade |
| B-22 | Política de retenção de dado de terceiro | D-13, depende de definição jurídica |
| B-23 | Certificado de conclusão | D-06 em aberto (vale como EPC?) |
| B-24 | Observabilidade: log estruturado, correlação, métrica de custo | |
| B-25 | Promover cada contexto a projeto próprio | Só se a disciplina de fronteira falhar (D-03) |

## P3 — ideias

| # | Item | Observação |
|---|---|---|
| B-26 | Exportar resposta do copiloto como minuta de parecer em Word | Sai da conversa e vira entregável para o cliente do contador |
| B-27 | Agrupar consultas por cliente atendido | O contador pensa por cliente, não por data |
| B-28 | Alerta quando norma nova afeta resposta já dada | Poderoso e caro; depende de vigência bem preenchida |
| B-29 | Segundo assento para funcionário do escritório | Modelo já suporta; falta interface e cobrança |
| B-30 | Marca d'água com e-mail do aluno no vídeo | Anti-vazamento |

---

## Concluídos

| Data | Item |
|---|---|
| 2026-09-05 | Esqueleto da solução, 5 projetos, referências e build limpo |
| 2026-09-05 | Documentação de produto, arquitetura, regras, glossário e decisões |
| 2026-09-05 | Configuração de Neon e R2 fora do versionamento |
| 2026-09-05 | Skills `nova-feature-backend`, `teste-integracao` e `revisao-pre-commit` |
| 2026-09-05 | **B-01** Praxis.Shared: Result, IRelogio, EntidadeBase |
| 2026-09-05 | **B-02** BancoDeTesteFixture com schema isolado e varredura de órfãos |
| 2026-09-05 | **B-03** DbContext, Npgsql e migration Inicial aplicada no Neon |
| 2026-09-05 | **B-04** Módulo Identidade: Organizacao, Usuario, PerfilDeUso |
| 2026-09-05 | **B-06** Módulo Assinaturas: DireitoDeUso e IConsultaDeDireitoDeUso |
| 2026-09-05 | Armazenamento em Cloudflare R2, Swagger, CORS e healthcheck |
| 2026-09-05 | 24 testes verdes (18 unidade, 6 integração) |
| 2026-09-05 | docs/deploy.md com as variáveis do Railway |
