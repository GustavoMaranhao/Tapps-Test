//Fonte: http://answers.unity3d.com/questions/357033/unity3d-and-c-coroutines-vs-threading.html
//Autor: Bunny83
//Acessado em: 21/09/2017
//Algumas modificações realizadas

public class ThreadedJob {
	private bool m_IsDone = false;
	private object m_Handle = new object();
	private System.Threading.Thread m_Thread = null;

	public bool IsDone {
		get	{
			bool tmp;
			lock (m_Handle)
				tmp = m_IsDone;
			return tmp;
		}

		set	{
			lock (m_Handle)
				m_IsDone = value;
		}
	}

	public virtual void Start()	{
		m_Thread = new System.Threading.Thread(Run);
		m_Thread.Start();
	}

	public virtual void Abort()	{
		m_Thread.Abort();
	}

	protected virtual void ThreadFunction() { }

	protected virtual void OnFinished() { }

	public virtual bool Update() {
		if (IsDone)	{
			OnFinished();
			return true;
		}
		return false;
	}

	private void Run()	{
		ThreadFunction();
		IsDone = true;
	}
}