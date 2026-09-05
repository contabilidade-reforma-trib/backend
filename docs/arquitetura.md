# Arquitetura — backend

## 1. Estilo

**Monolito modular.** Um processo, um banco, módulos com fronteira real. A escolha é deliberada: microsserviço a esta altura custaria operação sem entregar nada, e a fronteira entre pastas permite extrair um módulo depois sem reescrever o domínio.

## 2. Mapa dos módulos

| Módulo | Responde por | Não responde por |
|---|---|---|
| `Identidade` | Organização, usuário, login, papel | O que o usuário pode acessar |
| `Assinaturas` | Compra, direito de uso, vigência do acesso | Como cada produto usa o direito |
| `Copiloto` | Consulta, recuperação, resposta, fonte, custo | Conteúdo das aulas |
| `Mentoria` | Trilha, módulo, aula, progresso, material, certificado | Quem pode assistir |

`Assinaturas` é a única autoridade sobre acesso. Todo módulo pergunta, nenhum deduz.

## 3. Estrutura de pastas

```
src/Praxis.Modules/<Contexto>/
  Domain/           entidades, objetos de valor, regras e invariantes
  Application/      casos de uso, contratos públicos, interfaces de repositório
  Infrastructure/   EF Core, repositórios, clientes externos, mapeamentos
```

`Praxis.Shared` guarda o que é genuinamente comum: `Result`, tipos base de entidade, `IRelogio`, `IProvedorDeIa`, `IArmazenamentoDeObjetos`. **Não** é depósito de sobra — se só um módulo usa, mora no módulo.

## 4. Modelo de dados (esboço)

Nomes finais são decididos ao implementar cada módulo; este é o esqueleto conceitual.

```
Identidade
  Organizacao(Id, RazaoSocial, Documento, CriadoEm)
  Usuario(Id, OrganizacaoId, Nome, Email, Papel, CriadoEm)
  PerfilDeUso(UsuarioId, AreaDeAtuacao, RegimePredominante, Setores[], DorAtual)

Assinaturas
  Assinatura(Id, OrganizacaoId, Situacao, CriadoEm)
  DireitoDeUso(Id, AssinaturaId, Produto[Copiloto|Mentoria], InicioEm, FimEm)
  Pagamento(Id, AssinaturaId, Meio, Valor, Situacao)   -- simulado na POC

Copiloto
  Consulta(Id, UsuarioId, Pergunta, Modo, CriadoEm)
  Resposta(Id, ConsultaId, Texto, ModeloUsado, TokensEntrada, TokensSaida, CustoEstimado)
  FonteCitada(Id, RespostaId, TrechoId, Ordem)
  Documento(Id, Titulo, Tipo[Norma|MaterialDoMentor|Transcricao], Versao,
            VigenciaInicio?, VigenciaFim?, Situacao[Indexado|Indexando|Aposentado])
  Trecho(Id, DocumentoId, Conteudo, Embedding vector(1536), Ordem, Referencia)
  AvaliacaoDeResposta(Id, RespostaId, UsuarioId, Util, Comentario)

Mentoria
  Trilha(Id, Titulo, Assunto, Ordem, Situacao)
  Modulo(Id, TrilhaId, Titulo, Ordem)
  Aula(Id, ModuloId, Titulo, Ordem, DuracaoSegundos, ChaveVideo, Situacao)
  Transcricao(Id, AulaId, Texto, Marcacoes[])
  Material(Id, AulaId, Titulo, Tipo, ChaveArquivo, TamanhoBytes)
  ProgressoDeAula(UsuarioId, AulaId, SegundosAssistidos, ConcluidaEm?)
```

`Trecho` é a ponte entre os dois produtos: a transcrição de uma aula da Mentoria vira `Documento` do tipo `Transcricao` e passa a ser citável pelo Copiloto. É por isso que os produtos são independentes na venda, mas se reforçam no uso.

## 5. Esteira de ingestão

Nenhuma etapa roda dentro do request HTTP.

```
upload do vídeo (R2)
  → transcode / empacotamento HLS
  → transcrição com marcação de tempo        [em espera: Whisper não contratado]
  → fatiamento em trechos com referência
  → embedding
  → gravação em pgvector
  → disponível para citação
```

Cada etapa é idempotente e registra situação na entidade. Reprocessar um documento não duplica trecho.

Na POC a fila pode ser uma tabela `TarefaDeIngestao` com um worker em `BackgroundService`. Fila gerenciada só quando doer.

## 6. Recuperação (RAG)

1. Pergunta chega com o **perfil do usuário** (regime, setor, UF) como contexto.
2. Busca por similaridade em `Trecho`, filtrando por **vigência** e situação `Indexado`.
3. Reordenação, priorizando `MaterialDoMentor` e `Transcricao` sobre `Norma` — o produto vende prática, não texto de lei.
4. Montagem do prompt com os trechos e suas referências.
5. Geração da resposta com citação obrigatória por afirmação.
6. Se nenhum trecho passar do score mínimo, **a resposta é "não sei"** e a pergunta entra na fila de revisão. Isso é regra, não fallback.
7. Gravação de tokens e custo.

## 7. Infraestrutura

| Peça | Escolha | Observação |
|---|---|---|
| Banco | Neon (Postgres + pgvector) | Mesmo banco para dado relacional e vetorial na POC |
| Objetos | Cloudflare R2 | S3-compatible; SDK da AWS para .NET funciona direto |
| Vídeo | a decidir | R2 guarda; entrega com HLS e URL assinada precisa de decisão (ver D-08) |
| API | Railway | Deploy por git |
| Front | Vercel | Repositório separado |
| IA | a contratar | Abstraído por `IProvedorDeIa` desde o início |

## 8. Segurança mínima da POC

- URL de vídeo e de material é assinada e expira. Nunca link público de bucket.
- Toda requisição valida direito de uso no módulo `Assinaturas`.
- Dado que o contador digita sobre cliente dele não vai para treino de modelo e tem retenção definida.
