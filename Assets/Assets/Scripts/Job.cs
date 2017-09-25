using UnityEngine;

public class Job : ThreadedJob {
	public playerClass[] players;
	public propertyStruct[] props;

	public int numberOfTurns;
	public int maxNumberOfTurns;
	public int numberOfRounds;

	public int roundsTimedEnd = 0;

	private int absoluteTurnsNumber = 0;
	public float avgTurnsNumber = 0;

	private int absoluteVictoryRandom = 0;
	public float percentVictoryRandom = 0;

	private int absoluteVictoryCautious = 0;
	public float percentVictoryCautious = 0;

	private int absoluteVictoryDemanding = 0;
	public float percentVictoryDemanding = 0;

	private int absoluteVictoryImpulsive = 0;
	public float percentVictoryImpulsive = 0;

	protected override void ThreadFunction() {
		int currentRound = 0;
		//Laço principal que controla o número de rounds
		while (currentRound < numberOfRounds) {
			//Prepara o início da partida
			int currentTurn = 0;

			//Prepara os jogadores
			for (int i = 0; i < players.Length; i++) {
				players [i].setPlayerCoins(players [i].getPlayerStartingCoins ());
				players [i].setPlayerPos (0);
				players [i].setPlayerIsPlaying (true);
				players [i].setPlayerPropsReleased (false);
			}

			//Prepara as propriedades
			for (int i = 0; i < props.Length; i++) {
				props [i].owner = null;
			}

			//Inicia o jogo, só acaba depois que passar o número máximo de turnos ou só existir um jogador com coins
			while ((currentTurn < maxNumberOfTurns) && (!checkOnlyOneLeft())) {
				//Randomizar a ordem dos players
				for (int i = 0; i < players.Length; i++) {
					System.Random rndSeed = new System.Random();
					int rnd = rndSeed.Next(0, players.Length);
					playerClass tempPlayer = players[rnd];
					players[rnd] = players[i];
					players[i] = tempPlayer;
				}



				//Cada jogador executa seu turno
				for (int i = 0; i < players.Length; i++) {
					//Se o player não puder jogar, verifica se as suas propriedades foram devolvidas e pula a vez
					if (!players [i].getPlayerIsPlaying ()) {
						if (!players [i].getPlayerPropsReleased ()) {
							//Marca propriedades como devolvidas se forem do player
							for (int j = 0; j < props.Length; j++) {
								if (props [j].owner == players [i])
									props [j].owner = null;
							}

							//Marca que o player devolveu as propriedades
							players [i].setPlayerPropsReleased (true);
						}

						continue;
					}

					//Rolar um d6 e mover-se
					System.Random rnd = new System.Random();
					int diceRoll = rnd.Next (1, 7);
					players [i].movePlayer (diceRoll);

					//Se o player estiver em uma casa diferente da inicial
					if (players [i].getPlayerPos () > 0) {
						//Verifica se a casa tem dono
						if (props [players [i].getPlayerPos ()].owner == null) {
							//Grava os valores na propriedade caso o player decida comprar e deduz suas moedas
							if (players [i].decidePurchase (props [players [i].getPlayerPos ()])) {
								props [players [i].getPlayerPos ()].owner = players [i];
								players [i].changeCoins (-props [players [i].getPlayerPos ()].valueSale);
							}
						} else {
							//Se o dono não for o próprio player
							if (props [players [i].getPlayerPos ()].owner != players [i]) {
								//Pega o valor da propriedade
								int ammount = props [players [i].getPlayerPos ()].valueRent;

								//Remove as coins do player que caiu na casa e dá ao outro player
								players[i].changeCoins(-ammount);
								props [players [i].getPlayerPos ()].owner.changeCoins(ammount);
							}
						}
					}
				}

				//Guarda o número de turnos
				absoluteTurnsNumber++;

				//Turno acabou, começar o próximo
				currentTurn++;
			}
				
			if (currentTurn >= maxNumberOfTurns) roundsTimedEnd++;

			//Verifica qual player ganhou a partida
			switch(checkRoundWinner().getPlayerBehaviour()){
				case behaviourEnum.Aleatorio:
					absoluteVictoryRandom++;
					break;

				case behaviourEnum.Cauteloso:
					absoluteVictoryCautious++;
					break;

				case behaviourEnum.Exigente:
					absoluteVictoryDemanding++;
					break;

				case behaviourEnum.Impulsivo:
					absoluteVictoryImpulsive++;
					break;

				default:
					break;
			}

			//Partida acabou, começar a próxima
			currentRound++;
		}

		//Calcula os resultados finais
		avgTurnsNumber = absoluteTurnsNumber/currentRound;
		percentVictoryRandom = 100*absoluteVictoryRandom/currentRound;
		percentVictoryCautious = 100*absoluteVictoryCautious/currentRound;
		percentVictoryDemanding = 100*absoluteVictoryDemanding/currentRound;
		percentVictoryImpulsive = 100*absoluteVictoryImpulsive/currentRound;
	}

	private bool checkOnlyOneLeft(){
		int playersInGame = 0;

		//Verifica quantos players tem mais de 0 coins
		for (int i = 0; i < players.Length; i++) {
			if (players [i].getPlayerIsPlaying ())
				playersInGame++;
		}

		//Retorna true se houver apenas um player que possa jogar
		return (playersInGame <= 1);
	}

	private playerClass checkRoundWinner(){
		//A princípio, o vencedor será o primeiro a jogar, assim sempre teremos um valor de coins a ser comparado
		playerClass highestCoinsPlayer = players[0];

		//O vencedor será o player que tiver mais moedas ao final, independente da condição que fez o jogo acabar
		for (int i = 0; i < players.Length; i++) {
			//Mantém o player anterior caso o número de moedas seja igual
			if (players [i].getPlayerCoins () > highestCoinsPlayer.getPlayerCoins ())
				highestCoinsPlayer = players [i];
		}

		return highestCoinsPlayer;
	}

 	protected override void OnFinished() {
		//Escreve as respostas
		string mostVictories = "";
		if ((absoluteVictoryRandom >= absoluteVictoryCautious) &&
			(absoluteVictoryRandom >= absoluteVictoryDemanding) &&
			(absoluteVictoryRandom >= absoluteVictoryImpulsive))
		mostVictories = behaviourEnum.Aleatorio.ToString();

		if ((absoluteVictoryCautious >= absoluteVictoryRandom) &&
			(absoluteVictoryCautious >= absoluteVictoryDemanding) &&
			(absoluteVictoryCautious >= absoluteVictoryImpulsive))
		mostVictories = behaviourEnum.Cauteloso.ToString();

		if ((absoluteVictoryDemanding >= absoluteVictoryCautious) &&
			(absoluteVictoryDemanding >= absoluteVictoryRandom) &&
			(absoluteVictoryDemanding >= absoluteVictoryImpulsive))
		mostVictories = behaviourEnum.Exigente.ToString();

		if ((absoluteVictoryImpulsive >= absoluteVictoryCautious) &&
			(absoluteVictoryImpulsive >= absoluteVictoryDemanding) &&
			(absoluteVictoryImpulsive >= absoluteVictoryRandom))
		mostVictories = behaviourEnum.Impulsivo.ToString();

		GameObject.Find ("Answer 1").GetComponent<UnityEngine.UI.Text>().text =  avgTurnsNumber.ToString();
		GameObject.Find ("Answer 2").GetComponent<UnityEngine.UI.Text>().text = roundsTimedEnd.ToString();
		GameObject.Find ("Answer 3").GetComponent<UnityEngine.UI.Text>().text = mostVictories;

		GameObject.Find ("Answer 4").GetComponent<UnityEngine.UI.Text>().text = behaviourEnum.Aleatorio.ToString() + " " + percentVictoryRandom.ToString()+"%\n";
		GameObject.Find ("Answer 4").GetComponent<UnityEngine.UI.Text>().text += behaviourEnum.Cauteloso.ToString() + " " + percentVictoryCautious.ToString()+"%\n";
		GameObject.Find ("Answer 4").GetComponent<UnityEngine.UI.Text>().text += behaviourEnum.Exigente.ToString() + " " + percentVictoryDemanding.ToString()+"%\n";
		GameObject.Find ("Answer 4").GetComponent<UnityEngine.UI.Text>().text += behaviourEnum.Impulsivo.ToString() + " " + percentVictoryImpulsive.ToString()+"%";
	}
}