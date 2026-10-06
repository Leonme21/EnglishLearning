namespace EnglishLearning.Client.Services;

public class UserSession
{
    public int UsuarioId { get; private set; }
    public string NivelActual { get; private set; } = "SinAsignar";
    
    public event Action? OnChange;

    public void SetUser(int id, string nivel)
    {
        UsuarioId = id;
        NivelActual = nivel;
        NotifyStateChanged();
    }

    public void SetNivel(string nivel)
    {
        NivelActual = nivel;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
