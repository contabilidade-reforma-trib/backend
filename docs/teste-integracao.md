# Teste de integração com schema isolado

## Regra

Teste de integração roda no **mesmo banco Neon**, mas em **schema próprio por execução**, nunca no `public`. O schema é criado no início, recebe as migrations, e é derrubado no fim — **inclusive quando o teste falha ou estoura no meio**.

## Contrato da fixture

`tests/Praxis.IntegrationTests/Infra/BancoDeTesteFixture.cs` deve garantir:

1. **Criar** `teste_<aaaaMMddHHmmss>_<guid>` ao subir. O instante vai no nome de propósito: é o que permite a varredura de órfãos saber a idade do schema.
2. **Criar as tabelas** nesse schema a partir do modelo, com `Database.GenerateCreateScript()`.

   > Não use `EnsureCreated`: ele decide pela existência do **banco**, e o banco já existe. O schema novo continuaria vazio e todo insert falharia com *relation does not exist*. Migrations também não servem aqui, porque o `__EFMigrationsHistory` é por banco, não por schema.

3. Fornecer o `DbContext` com `HasDefaultSchema` apontando para ele.
4. **Derrubar** com `DROP SCHEMA <nome> CASCADE` em `DisposeAsync`, dentro de `try/finally`.
5. Ao subir, **varrer schemas órfãos**: derrubar todo `teste_%` criado há mais de 24h. Rede de segurança para execução interrompida por queda de máquina.

Ponto crítico: a derrubada tem que sobreviver a exceção no meio da suíte. Se `DisposeAsync` puder não rodar no seu arranjo, embrulhe a execução em `try/finally` você mesmo. **Schema órfão acumulando no Neon é falha de implementação da fixture, não acidente.**

Configuração em `appsettings.json`:

```json
"TestesIntegracao": { "PrefixoSchema": "teste_", "DerrubarSchemaAoFinal": true }
```

Nunca versione `DerrubarSchemaAoFinal: false`. Para inspecionar dados, desligue localmente e reverta antes de terminar.

## O que testar aqui

- Que a migration realmente cria o que o mapeamento espera.
- Que a consulta retorna o que deveria, com dado real no banco.
- Que a regra de acesso barra quem não tem direito de uso.
- Fluxo que atravessa mais de uma camada.

## O que não testar aqui

- Regra de domínio pura — isso é teste de unidade, é mais rápido e não precisa de banco.
- Que o EF salva. Testar framework é desperdício.

## Serviço externo

R2 e provedor de IA **não** são chamados de verdade em teste. Use implementação falsa da abstração (`IArmazenamentoDeObjetos`, `IProvedorDeIa`). Se você sentir vontade de chamar a API real para "ter certeza", o que falta é um teste manual documentado, não um teste automatizado instável.

## Nomes

Português, descrevendo comportamento e resultado:

```
Deve_derrubar_o_schema_mesmo_quando_o_teste_falha
Deve_recusar_consulta_quando_organizacao_nao_tem_direito_de_uso
```

## Antes de terminar

```bash
dotnet test
```

Rode duas vezes seguidas. Se a segunda falhar, a limpeza está incompleta — conserte a fixture antes de seguir.
