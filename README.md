# Tapps-Test

Instruções de uso:
 - O aplicativo possui apenas uma tela, na qual o usuário deve inserir os valores que deseja no painel mais à esquerda da tela para que a  simulação possa ser realizada;
 - Caso nenhum parâmetro seja inserido, o programa assumirá os valores padrão definidos durante o desenvolvimento do trabalho;
 - Depois de inseridos os valores desejados deve-se clicar no botão "Carregar Informações" para que o aplicativo carregue os valores desejados pelo usuário nas variáveis de simulação;
 - Com os valores carregados deve-se clicar no botão "Iniciar Simulação" e aguardar os resultados;
 - Ao clicar no botão "Iniciar Simulação", sua funcionalidade e visualização será alterada para "Parar Simulação" até que o aplicativo termine de calcular a simulação, caso este botão seja clicado antes do término do processo os valores serão descartados e o painel de resultados continuará com os resultados anteriores;
 - Depois de terminada, a simulação pode ser iniciada quantas vezes o usuário desejar, com os mesmos parâmetros ou não, apenas clicando no botão de início novamente;

Componentes do aplicativo:
 - O painel mais à esquerda da tela é o painel de preferências, o qual possui os campos que interajem com o usuário e botões de controle da simulação, conforme explicados acima;
 - O painel central possui uma lista de jogadores e quantas coins terão no início de cada rodada na coluna mais à esquerda após o usuário clicar no botão de carregamento das informações no painel de preferências. Depois de iniciada a simulação pelo menos uma vez, no lado direito do painel aparecerá uma coluna com a ordem de jogo dos players para cada simulação;
 - O painel mais à direita da tela possui as informações das casas no tabuleiro, este painel será preenchido com os valores do arquivo gameConfig.txt depois que o usuário clicar no botão de carregamento no painel de preferências;
 - Abaixo dos painéis existe a área de respostas, onde depois de cada simulação o aplicativo escreverá os resultados pertinentes ao teste;
