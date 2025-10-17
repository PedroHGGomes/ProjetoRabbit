PROJETORABBIT - CHECKPOINT 5 FIAP
Disciplina: Programação de API com Microsserviços (.NET + RabbitMQ)
Turma: 2TDSX

-------------------------------------------
DESCRIÇÃO DO PROJETO
-------------------------------------------
O ProjetoRabbit demonstra a comunicação assíncrona entre microsserviços utilizando o RabbitMQ como broker de mensagens, desenvolvido em .NET 8 Console Application.

A aplicação simula o fluxo de mensagens entre três contextos principais:
- Sender (Produtor): envia mensagens de frutas e usuários.
- Validation (Processador): consome, valida e repassa mensagens validadas.
- Receiver (Consumidor): recebe e exibe as mensagens validadas.

-------------------------------------------
INTEGRANTES
-------------------------------------------
Pedro Gomes - RM553907
Luiz Felipe Abreu - RM555197
Matheus Munuera - RM557812

-------------------------------------------
TECNOLOGIAS UTILIZADAS
-------------------------------------------
- .NET 8.0 (Console App)
- C#
- RabbitMQ.Client 6.8.1
- Docker + Docker Compose
- RabbitMQ Management Plugin (porta 15672)

-------------------------------------------
ESTRUTURA DO PROJETO
-------------------------------------------
ProjetoRabbit/
│
├── ProjetoRabbit.sln
├── docker-compose.yml
├── integrantes.txt
├── README.txt
└── ProjetoRabbit/
    ├── ProjetoRabbit.csproj
    ├── Program.cs
    ├── bin/
    └── obj/

-------------------------------------------
COMO EXECUTAR O PROJETO
-------------------------------------------
1. Pré-requisitos:
   - Docker Desktop instalado e em execução
   - .NET SDK 8.0 instalado
   - Visual Studio 2022 (opcional)

2. Subir o container do RabbitMQ:
   docker compose up -d

   Isso criará o container 'rabbitmq' com as portas:
   - 5672 → comunicação com o .NET
   - 15672 → painel de administração

   Acesse o painel:
   http://localhost:15672
   Login: guest
   Senha: guest

3. Restaurar pacotes e compilar o projeto:
   dotnet restore
   dotnet build
   dotnet run

-------------------------------------------
MENU DA APLICAÇÃO
-------------------------------------------
=== ProjetoRabbit (.NET 8 + RabbitMQ) ===
[1] Enviar FRUTAS
[2] Enviar USUÁRIOS
[3] VALIDATION (verifica e repassa)
[4] Receiver FRUTAS
[5] Receiver USUÁRIOS
[0] Sair

-------------------------------------------
FLUXO DE MENSAGENS
-------------------------------------------
1. Sender publica mensagens nas exchanges.
2. Validation consome as filas de validação e verifica os dados.
3. Após validar, publica nas exchanges de mensagens validadas.
4. Receiver consome das filas finais e exibe o resultado.

-------------------------------------------
EXEMPLO DE EXECUÇÃO
-------------------------------------------
Janela 1 (Validation):
dotnet run
Escolha: 3

Janela 2 (Sender):
dotnet run
Escolha: 1  (Envia frutas)

Janela 3 (Receiver):
dotnet run
Escolha: 4  (Recebe frutas validadas)

-------------------------------------------
DIAGRAMA SIMPLIFICADO
-------------------------------------------
Sender -> [exchange.fruits] -> [queue.fruit.validation]
                 |
                 v
           Validation (verifica)
                 |
                 v
        [exchange.validated.fruits]
                 |
                 v
           Receiver (exibe dados)

-------------------------------------------
FINALIZAÇÃO
-------------------------------------------
Para encerrar o RabbitMQ:
docker compose down

-------------------------------------------
CONCLUSÃO
-------------------------------------------
O projeto demonstra na prática o uso de microsserviços com comunicação assíncrona via RabbitMQ, 
possibilitando integração entre componentes de forma escalável e desacoplada.
