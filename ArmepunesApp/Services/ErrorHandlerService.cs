namespace ArmepunesApp.Services;

public static class ErrorHandlerService
{
    public static void HandleException(Exception ex, string konteksti, string? perdoruesi = null, string? vendndodhja = null)
    {
        var msg = $"Gabim gjate {konteksti}";
        if (!string.IsNullOrEmpty(perdoruesi))
            msg += $" - Perdoruesi: {perdoruesi}";
        if (!string.IsNullOrEmpty(vendndodhja))
            msg += $" - Vendndodhja: {vendndodhja}";

        MessageBox.Show($"{msg}\n\nDetaje: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static void HandleException(Exception ex, string konteksti, object perdoruesi)
    {
        string? username = null;
        if (perdoruesi != null)
        {
            var prop = perdoruesi.GetType().GetProperty("Username");
            if (prop != null)
                username = prop.GetValue(perdoruesi)?.ToString();
        }
        HandleException(ex, konteksti, username);
    }
}
