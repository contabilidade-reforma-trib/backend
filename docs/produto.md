# Produto

> Este arquivo é **idêntico** nos repositórios `backend` e `frontend`. Ao alterar um, replique no outro.

## 1. Para quem

Contadores brasileiros — dono de escritório contábil, contador de time fiscal interno e consultor tributário — que precisam atender clientes durante a **Reforma Tributária do consumo** (EC 132/2023 e legislação complementar: IBS, CBS e Imposto Seletivo).

O usuário é o contador. **O cliente do contador não acessa a plataforma e não sabe que ela existe.** Não há área do cliente final, não há white label, não há convite para terceiros.

## 2. Quem ensina

Duas profissionais atuantes, sócias do produto:

- **Contadora especializada em tributário** — já conduz mentorias hoje; é a origem do material prático e dos casos reais.
- **Advogada tributarista especializada em Reforma Tributária** — responde pela fundamentação normativa.

A autoridade delas é argumento central de venda: o material não é resumo de lei, é procedimento testado em cliente real.

## 3. Os dois produtos

São **produtos independentes**, vendidos separadamente, sob o mesmo login e a mesma conta.

### 3.1 Copiloto

Assistente de IA que responde dúvidas práticas de contabilidade tributária consultando a base de documentação e experiência das mentoras (RAG).

- Responde **como resolver o problema**, não o que a lei diz.
- **Toda afirmação carrega citação verificável.** Sem fonte na base, o copiloto declara que não sabe — nunca improvisa. Esta é a regra de negócio mais importante do produto inteiro.
- Modos de resposta que mudam o formato, não o conteúdo: **Prático** (passo a passo), **Cálculo** (tabela/simulação) e **Norma** (fundamentação).
- Histórico de consultas, organizável por cliente atendido.

### 3.2 Mentoria

Plataforma de ensino em vídeo sobre o **assunto** — reforma tributária aplicada. **Não** ensina a usar o copiloto e **não** pressupõe que o aluno tenha o copiloto.

Estruturalmente é um sistema inteiro dentro do sistema, com navegação própria: trilhas, módulos, aulas, progresso, materiais para baixar, plantões ao vivo, certificado. Trate a Mentoria como aplicação completa com submenus próprios, não como uma lista de vídeos.

Hierarquia: **Trilha → Módulo → Aula**. Aula tem vídeo, transcrição e anexos (planilha, modelo de petição, checklist).

## 4. Modelo de venda

Quatro caminhos possíveis, todos válidos:

| Caminho | Situação |
|---|---|
| Só Copiloto | Quer a ferramenta do dia a dia, não quer estudar agora |
| Só Mentoria | Quer a formação, não quer (ou não confia ainda em) IA |
| Combo | Compra os dois de uma vez, com preço melhor |
| Incremental | Compra um, adiciona o outro depois — sem perder histórico nem progresso |

Consequência técnica: **acesso é por direito de uso (entitlement), não por plano fixo.** Uma conta pode ter direito ao Copiloto, à Mentoria, ou aos dois, com datas de início e fim independentes. O módulo `Assinaturas` é a única autoridade sobre isso; `Copiloto` e `Mentoria` perguntam a ele, nunca deduzem.

Pagamento na POC é **simulado**. Quando for real: Pix e boleto são obrigatórios para este público — cartão sozinho não atende.

## 5. Conta e organização

O contador compra para si. Mas ele pode comprar e colocar funcionários do escritório para usar.

**Decisão tomada:** a **conta é a contabilidade (organização)**; os usuários são pessoas dentro dela. Mesmo o contador autônomo é uma organização de um usuário só. Isso evita a migração cara de transformar conta pessoal em conta de empresa depois.

- `Organizacao` — a contabilidade. Dona da assinatura e dos direitos de uso.
- `Usuario` — pessoa que faz login, pertence a uma organização, tem papel (`Proprietario`, `Membro`).
- Histórico de consultas e progresso de aulas são **do usuário**, não da organização.

## 6. Jornada

1. **Landing** — página de vendas. Apresenta os dois produtos, as mentoras, as trilhas, prova social, oferta e garantia.
2. **Compra** (simulada na POC) — escolhe Copiloto, Mentoria ou Combo.
3. **Cadastro → trilha** — quatro perguntas de perfil (onde atua, regime predominante da carteira, setores, dor atual). O resultado ordena as trilhas da Mentoria e vira contexto do Copiloto.
4. **Login → destino** — quem tem Copiloto cai no Copiloto; quem só tem Mentoria cai na Mentoria.
5. **Menu superior** — alterna entre Copiloto e Mentoria. Produto não contratado aparece como oferta, não some.

## 7. Administração

Área para as mentoras, separada do app do aluno:

- Criar e ordenar trilhas, módulos e aulas.
- Subir vídeo. O upload dispara a esteira: **upload → transcode → transcrição → fatiamento → indexação**. A transcrição vira fonte citável do copiloto.
- Gerir a base de conhecimento do copiloto: documentos, versão e **vigência**.
- Fila de revisão: resposta marcada como errada pelo aluno volta para a mentora.

## 8. Regras de produto que viram regra de código

1. **Sem fonte, sem resposta.** O copiloto não afirma nada que não esteja na base.
2. **Vigência.** A reforma é escalonada até 2033: uma resposta correta em 2026 fica errada em 2029. Todo trecho indexado carrega vigência (`VigenciaInicio`, `VigenciaFim`, ambas opcionais). Documento aposentado sai da recuperação sem ser apagado.
3. **Acesso é por direito de uso**, verificado a cada requisição, nunca inferido do plano comprado.
4. **Dado de terceiro.** O contador vai colar CNPJ, faturamento e nome de clientes dele no chat. Isso é dado de terceiro sob LGPD: não é usado para treinar modelo, não sai da conta, e tem prazo de retenção definido.
5. **Custo por consulta** é gravado, porque "consultas ilimitadas" precisa de um teto real conhecido.
