using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

//Enum com os possíveis comportamentos dos players
public enum behaviourEnum{
	Impulsivo,
	Exigente,
	Cauteloso,
	Aleatorio
}

//Struct com as propriedades individuais de cada casa no tabuleiro
public struct propertyStruct{
	public int id;
	public int valueSale;
	public int valueRent;
	public playerClass owner;
	public bool isStart;
}

//Classe que definirá o comportamento do player
public class playerClass{
	private int boardNumberOfHouses;
	private int boardCircuitReward;

	//TODO remover myPlayerId
	private int myPlayerId;
	private bool isPlaying;
	private behaviourEnum myBehaviour;
	private bool myPropsReleased;


	private int myStartingCoins;
	private int myCoins;
	private int myPos;

	//Construtor
	public playerClass(){
		myStartingCoins = 0;
		myCoins = 0;
		myPos = 0;

		myPropsReleased = false;
	}

	//Método auxiliar para o player saber quando deu uma volta no tabuleiro
	public void setBoardNumberOfHouses(int houses) { boardNumberOfHouses = houses; }

	//Método auxiliar para o player saber quando recebe por uma volta no tabuleiro
	public void setBoardCircuitReward(int coins) { boardCircuitReward = coins; }

	//Métodos auxiliares para o id do player
	public int getPlayerId(){ return myPlayerId; }
	public void setPlayerId(int playerId){ myPlayerId = playerId; }

	//Métodos auxiliares para saber se o player ainda está jogando
	public bool getPlayerIsPlaying(){ return isPlaying; }
	public void setPlayerIsPlaying(bool amPlaying){ isPlaying = amPlaying; }

	//Métodos auxiliares para o comportamento do player
	public behaviourEnum getPlayerBehaviour(){ return myBehaviour; }
	public void setPlayerBehaviour(behaviourEnum newBehaviour){ myBehaviour = newBehaviour; }

	//Métodos auxiliares para saber se o player liberou suas propriedades ao perder
	public bool getPlayerPropsReleased(){ return myPropsReleased; }
	public void setPlayerPropsReleased(bool propsReleased){ myPropsReleased = propsReleased; }

	//Métodos auxiliares para as coins iniciais do player
	public int getPlayerStartingCoins(){ return myStartingCoins; }
	public void setPlayerStartingCoins(int newCoins){ myStartingCoins = newCoins; }

	//Métodos auxiliares para as coins do player
	public int getPlayerCoins(){ return myCoins; }
	public void setPlayerCoins(int newCoins){ myCoins = newCoins; }

	//Métodos auxiliares para a posição do plyaer do player
	public int getPlayerPos(){ return myPos; }
	public void setPlayerPos(int newPos){ myPos = newPos; }

	public void movePlayer(int ammount){
		//Verifica se o player passou pelo começo
		if (getPlayerPos () + ammount >= boardNumberOfHouses) {
			//Se passou, adiciona 100 coins ao player e soma o quanto falta para o player andar
			setPlayerCoins (getPlayerCoins() + boardCircuitReward);
			//A nova posição será o atual + dado - número de casas - ajuste devido à casa 0
			setPlayerPos (getPlayerPos() + ammount - boardNumberOfHouses);
		} else {
			//Se não passou ainda, apenas soma o quanto o player andou
			setPlayerPos (getPlayerPos () + ammount);
		}
	}

	public void changeCoins(int ammount){
		setPlayerCoins (getPlayerCoins() + ammount);

		//Se o player não tiver mais coins, não joga mais
		if (getPlayerCoins () <= 0)
			setPlayerIsPlaying (false);
	}

	public bool decidePurchase(propertyStruct propToBuy){
		//Verifica se o player tem coins o suficiente para comprar a propriedade
		if (getPlayerCoins () < propToBuy.valueSale)
			return false;

		//Verifica o comportamento do player
		switch (getPlayerBehaviour()){
			case behaviourEnum.Aleatorio:
				//Este comportamento compra com uma possibilidade de 50%
				System.Random rnd = new System.Random ();
				int chance = rnd.Next (0, 101);
				if (chance >= 50)
					return true;
				else
					return false;

			case behaviourEnum.Cauteloso:
				//Este comportamento compra caso sobrem 80 coins ou mais após a compra
				if (getPlayerCoins () - propToBuy.valueSale >= 80)
					return true;
				else
					return false;

			case behaviourEnum.Exigente:
				//Este comportamento compra caso o aluguel seja maior que 50 coins
				if (propToBuy.valueRent >= 50)
					return true;
				else
					return false;

			case behaviourEnum.Impulsivo:
				//Este comportamento compra qualquer propriedade que em parar
				return true;

			default:
				//Valor padrão será a não compra
				return false;
		}
	}
}

//Classe principal do Manager
public class GameManager : MonoBehaviour {	
	private propertyStruct[] props = new propertyStruct[21];
	
	private playerClass[] players = new playerClass[4];
	private playerClass[] turnOrder;

	private GameObject btnStart;
	private GameObject btnLoad;
	private GameObject fieldCoins;
	private GameObject fieldRounds;
	private GameObject fieldFile;
	private GameObject memoProps;
	private GameObject memoPlayers;
	private GameObject memoPlayerOrder;

	private Job gameJobThread = null;

	private int startCoins = 300;
	private int numberRounds = 300;
	private string configFile = "C:/gameConfig.txt";

	private int numberOfPlayers = 4;
	private int maxNumberOfTurns = 1000;
	private int coinsRewardPerCompleteCircuit = 100;

	private bool bIsGameRunning = false;

	void Start () {
		//configFile = Application.dataPath;

		//Referências para os objetos que serão necessários
		btnStart = GameObject.Find ("btnStart");
		btnLoad = GameObject.Find ("btnLoad");
		fieldCoins = GameObject.Find ("inCoins");
		fieldRounds = GameObject.Find ("inRounds");
		fieldFile = GameObject.Find ("inFile");
		memoProps = GameObject.Find ("memoProps");
		memoPlayers = GameObject.Find ("memoPlayers");
		memoPlayerOrder = GameObject.Find ("memoPlayerOrder");

		//O jogo ainda não começou
		bIsGameRunning = false;
	}

	void Update(){
		//Se existir alguma simulação em andamento
		if (gameJobThread != null) {
			//Verifica se acabou o processo a cada frame
			if (gameJobThread.Update ()) {				
				gameJobThread = null;

				//Volta ao estado inicial do jogo
				onStartClick();
			}
		}
	}

	//Ao clicar no botão "Carregar Informações"
	public void onLoadClick(){		
		loadFields ();

		//Tenta ler o arquivo
		//TODO Adicionar o else para o caso da file estiver vazia ainda
		if (configFile != "") {
			initializeProps ();

			//Define os players
			initializePlayers();

			//Atualiza exibićão do placar dos players
			refreshPlayersMemo();
		}

		//Permite começar a simulação
		btnStart.GetComponent<Button> ().interactable = true;
	}

	//Ao clicar no botão "Iniciar Simulação"
	public void onStartClick(){
		if (!bIsGameRunning) {
			//Desabilita edição de parâmetros
			togglePrefs (false);

			//Muda o texto do botão
			btnStart.GetComponentInChildren<Text> ().text = "Parar Simulação";

			//Prepara o início do jogo
			bIsGameRunning = true;

			//Reseta o vetor de turnos
			turnOrder = new playerClass[4];

			//Primeiro a jogar pode ser qualquer um
			int randomInt = Random.Range(0,4);
			turnOrder [0] = players [randomInt];

			//Define a ordem dos outros jogadores
			for (int i = 1; i < turnOrder.Length; i++) {
				bool bInserted = true;
				while (bInserted) {
					//Sorteia um jogador
					randomInt = Random.Range (0, 4);

					//Verifica se este jogador já foi inserido
					for (int j = 0; j < turnOrder.Length; j++) {
						if (turnOrder [j] == players [randomInt]) {
							//Se já achou algum player igual, pode sair do loop
							bInserted = true;
							break;
						}

						//Se chegou até o final o player ainda não foi inserido
						bInserted = false;
					}
				}

				//Insere o jogador na ordem do turno
				turnOrder [i] = players [randomInt];
			}

			//Exibe a ordem dos players
			Text memo = memoPlayerOrder.GetComponent<Text> ();
			memo.text = "Ordem da primeira partida do jogo: \n";
			for (int i = 0; i < turnOrder.Length; i++) {
				memo.text += "Jogador " + i + ": Player " + turnOrder [i].getPlayerBehaviour()+'\n';
			}

			//Prepara a thread do jogo
			gameJobThread = new Job();
			gameJobThread.players = turnOrder;
			gameJobThread.props = props;
			gameJobThread.numberOfTurns = numberRounds;
			gameJobThread.maxNumberOfTurns = maxNumberOfTurns;
			gameJobThread.numberOfRounds = numberRounds;

			//Inicia a thread do jogo
			gameJobThread.Start();
		} else {
			//Destrói a thread se ela for parada antes de terminar
			if (gameJobThread != null) {
				gameJobThread.Abort ();
				gameJobThread = null;
			}

			//Para o jogo
			bIsGameRunning = false;

			//Muda o texto do botão
			btnStart.GetComponentInChildren<Text> ().text = "Iniciar Simulação";

			//Apaga a ordem dos players
			memoPlayerOrder.GetComponent<Text> ().text = "";

			//Habilita edição de parâmetros
			togglePrefs (true);
		}
	}

	private void loadFields(){
		//Pega a quantidade de moedas iniciais definida ou escreve o valor default no campo
		InputField inCoins = fieldCoins.GetComponent<InputField> ();
		if (inCoins.text != "")
			startCoins = int.Parse (inCoins.text);
		else
			inCoins.text = startCoins.ToString();

		//Pega a quantidade de turnos iniciais definida ou escreve o valor default no campo
		InputField inRounds = fieldRounds.GetComponent<InputField> ();
		if (inRounds.text != "")
			numberRounds = int.Parse (inRounds.text);
		else
			inRounds.text = numberRounds.ToString();

		//Pega o local onde o arquivo gameConfig.txt está ou escreve o valor default no campo
		InputField inFile = fieldFile.GetComponent<InputField> ();
		if (inFile.text != "")
			configFile = inFile.text;
		else
			inFile.text = configFile;
	}

	private void initializePlayers(){
		//Define o tipo dos players e diz aos jogadores quantas casas o tabuleiro tem
		for (var i = 0; i < numberOfPlayers; i++) {
			players [i] = new playerClass ();
			players [i].setPlayerId (i);
			players [i].setBoardNumberOfHouses (props.Length);
			players [i].setPlayerStartingCoins (startCoins);
			players [i].setBoardCircuitReward (coinsRewardPerCompleteCircuit);
		}

		//Define o comportamento dos players
		players [0].setPlayerBehaviour(behaviourEnum.Impulsivo);
		players [1].setPlayerBehaviour(behaviourEnum.Exigente);
		players [2].setPlayerBehaviour(behaviourEnum.Cauteloso);
		players [3].setPlayerBehaviour(behaviourEnum.Aleatorio);
	}

	private void initializeProps(){
		//Grava os valores do arquivo em um array de strings
		StreamReader file = File.OpenText (configFile);
		string[] values = file.ReadToEnd ().Split ('\n');
		file.Close ();

		//Onde os valores serão escritos
		Text memo = memoProps.GetComponent<Text> ();

		//Adiciona a casa de início
		props [0].id = 0;
		props [0].valueSale = 0;
		props [0].valueRent = 0;
		props [0].owner = null;
		props [0].isStart = true;
		memo.text = "Índice: " + props [0].id.ToString("D2") + "   ";
		memo.text += "Venda: " + props [0].valueSale + "   ";
		memo.text += "Aluguel: " + props [0].valueRent + "   ";
		memo.text += "Início: " + props [0].isStart + '\n';

		//Transfere os valores do array para os componentes do jogo
		for (var i = 1; i < props.Length; i++) {
			props [i].id = i;
			props [i].valueSale = int.Parse(values [i-1].Substring (0, values [i-1].IndexOf (" ")));
			props [i].valueRent = int.Parse(values [i-1].Substring (values [i-1].IndexOf (" ") + 1, values[i-1].Length - values [i-1].IndexOf (" ")-1));
			props [i].owner = null;
			props [i].isStart = false;

			memo.text += "Índice: " + props [i].id.ToString("D2") + "   ";
			memo.text += "Venda: " + props [i].valueSale + "   ";
			memo.text += "Aluguel: " + props [i].valueRent + "   ";
			memo.text += "Início: " + props [i].isStart + '\n';
		}
	}

	private void refreshPlayersMemo(){
		Text memo = memoPlayers.GetComponent<Text> ();
		memo.text = "";

		for (var i = 0; i < players.Length; i++) {
			memo.text += "Player "+i+": " + players [i].getPlayerBehaviour() + "\n ";
			memo.text += "Moedas iniciais: " + players [i].getPlayerStartingCoins() + "\n\n";
		}
	}

	private void togglePrefs(bool toggle){
		fieldCoins.GetComponent<InputField> ().interactable = toggle;
		fieldRounds.GetComponent<InputField> ().interactable = toggle;
		fieldFile.GetComponent<InputField> ().interactable = toggle;
		btnLoad.GetComponent<Button> ().interactable = toggle;
	}
		
}
