# Cloudflare — R2 e Stream

Duas peças com papéis diferentes: **R2** guarda arquivo (documento, planilha, material de aula), **Stream** entrega vídeo. Vídeo não vai para o R2 — a razão está na seção 3.

---

## 1. R2 — região

### A resposta curta: não vale a pena mexer

O *location hint* do R2 é definido **na criação do bucket e não pode ser alterado**. Pior: recriar um bucket com o **mesmo nome** reaproveita a localização do original, então trocar exigiria buckets com **nome novo**.

E, diferente do banco, isso quase não afeta o produto:

| Operação | Toca o R2 pela rede? |
|---|---|
| Gerar URL assinada de leitura | **Não.** É assinatura criptográfica local, sem chamada de rede |
| Aluno baixando um material | Não passa pela nossa API — vai do navegador direto ao R2, pela rede da Cloudflare |
| Admin subindo documento | Sim, uma vez por arquivo |
| Ingestão para o RAG | Sim, uma vez por documento |

O Neon estava no caminho de **toda** requisição, por isso a troca de região valeu tanto (de ~1500 ms para 37 ms). O R2 é tocado em upload e em ingestão, operações raras e assíncronas, e a leitura pelo aluno nem passa por nós. A localização vira detalhe.

Some-se que *location hint* na Cloudflare é **melhor esforço**, não garantia, e que o R2 é servido pela rede global da Cloudflare de qualquer forma.

### Se ainda assim quiser alinhar

Só faz sentido agora, enquanto os buckets estão vazios. Crie buckets **com nome diferente** e *location hint* **ENAM** (Eastern North America), que é onde fica a Virgínia:

1. Painel da Cloudflare → **R2** → **Create bucket**
2. Nome novo — por exemplo `praxis-videos-enam`, `praxis-documentos-enam`
3. **Location** → *Specify location* → **ENAM**
4. Atualizar `ArmazenamentoObjetos__BucketVideos` e `__BucketDocumentos` no Railway e no `appsettings.Development.json`
5. Apagar os buckets antigos

Reusar o nome antigo **não** funciona: a localização original volta.

---

## 2. Stream — configuração

O Stream é um produto pago à parte, cobrado por minuto **armazenado** e por minuto **entregue**. Confirme os valores atuais na página de preços antes de subir a biblioteca inteira: o custo escala com a duração do acervo, não com o número de alunos.

### 2.1 Habilitar e pegar as credenciais

1. Painel da Cloudflare → **Stream** → habilitar (pede plano/cartão)
2. Anote o **Account ID** — é o mesmo que já usamos no R2: `ef76ef487ce9ad8b31acf91301e848a9`
3. **My Profile → API Tokens → Create Token → Custom token**
   - Permissão: **Account · Stream · Edit**
   - Escopo: a conta do projeto
   - Guarde o token: ele **não é exibido de novo**

### 2.2 Chave de assinatura (para vídeo pago)

Sem isso qualquer pessoa com o id do vídeo assiste, e o curso vaza no primeiro grupo de WhatsApp.

1. Gere uma **signing key** pela API do Stream (`/accounts/{account_id}/stream/keys`)
2. Guarde os dois valores devolvidos: o **id da chave** e a **chave privada em PEM**
3. Todo vídeo é criado com **`requireSignedURLs: true`** — assim o link público e o player embutido deixam de funcionar sem token

Há dois jeitos de emitir o token de acesso:

- **Endpoint `/token`** — uma chamada à Cloudflare por token. Simples, sujeito a limite de taxa. Serve para começar.
- **Assinar localmente com a chave privada** — sem chamada de rede, sem limite. É para onde vamos quando houver volume.

Comece pelo endpoint e troque quando incomodar. A abstração no código já isola isso.

### 2.3 Variáveis que vão entrar

Ainda **não existem no `appsettings.json`** — quando você tiver os valores, elas entram como chaves vazias no arquivo versionado e preenchidas no Railway:

```
Video__Provedor=CloudflareStream
Video__AccountId=ef76ef487ce9ad8b31acf91301e848a9
Video__ApiToken=<token com Stream:Edit>
Video__ChaveDeAssinaturaId=<id da signing key>
Video__ChaveDeAssinaturaPem=<chave privada PEM>
Video__MinutosDeValidadeDoToken=120
```

### 2.4 Como o upload vai funcionar

O vídeo **não** passa pela nossa API. Arquivo de 2 GB atravessando o Railway seria desperdício de banda e de tempo limite.

```
admin escolhe o arquivo
  → nossa API pede à Cloudflare uma URL de upload direto (tus)
  → navegador envia o arquivo direto para a Cloudflare
  → Cloudflare faz encoding e avisa por webhook quando termina
  → gravamos o videoId na Aula
  → transcrição alimenta o RAG
```

O que guardamos na `Aula` é só o **`videoId`**. A URL de reprodução é gerada na hora, assinada e com validade curta.

---

## 3. Por que vídeo no Stream e não no R2

O R2 guarda bytes. Um `.mp4` de 2 GB no R2 é entregue como um arquivo único: o aluno espera o download, não existe adaptação de qualidade, e quem tem internet ruim não assiste.

O Stream entrega **HLS**: fatia o vídeo, gera várias resoluções e o player troca conforme a banda. Faz o encoding sozinho, tem player pronto e assinatura de URL embutida.

Fazer isso sobre o R2 significaria montar transcode, empacotamento HLS e distribuição por conta própria — semanas de trabalho para reconstruir o que o Stream entrega ligado.

**Material de apoio continua no R2**: planilha, modelo de petição, checklist. São arquivos pequenos, baixados uma vez, sem qualquer necessidade de streaming.
