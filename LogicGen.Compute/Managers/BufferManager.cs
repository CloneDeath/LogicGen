namespace LogicGen.Compute.Managers; 

public class BufferManager : IDisposable {
	
	
	#region IDisposable
	private void ReleaseUnmanagedResources() {
		// TODO release unmanaged resources here
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~BufferManager() {
		ReleaseUnmanagedResources();
	}
	#endregion
}