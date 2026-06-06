using ArmepunesApp.Services;

namespace ArmepunesApp.Forms;

public partial class AiSettingsForm : Form
{
    private readonly AiSettings _settings;

    public AiSettingsForm()
    {
        _settings = AiSettings.Load();
        InitializeComponent();
        txtEndpoint.Text = _settings.Endpoint;
        txtModel.Text = _settings.Model;
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        var endpoint = txtEndpoint.Text.Trim();
        var model = txtModel.Text.Trim();

        if (string.IsNullOrEmpty(endpoint))
        {
            MessageBox.Show("Endpoint nuk mund te jete bosh.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrEmpty(model))
        {
            MessageBox.Show("Modeli nuk mund te jete bosh.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _settings.Endpoint = endpoint;
        _settings.Model = model;
        _settings.Save();

        MessageBox.Show("Cilësimet u ruajten me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }

    private async void btnTest_Click(object? sender, EventArgs e)
    {
        var endpoint = txtEndpoint.Text.Trim();
        var model = txtModel.Text.Trim();

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(model))
        {
            MessageBox.Show("Ploteso endpoint dhe modelin para testimit.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        lblTest.Enabled = false;
        lblTest.Text = "Duke testuar...";

        try
        {
            var testSettings = new AiSettings { Endpoint = endpoint, Model = model, TimeoutSeconds = 10 };
            var helper = new AiHelper(testSettings);
            var result = await helper.GjeneroFleteleshim("TEST: Pershendetje, kjo eshte nje test.");

            if (result.StartsWith("<Gabim"))
            {
                MessageBox.Show($"Lidhja deshtoi:\n{result}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show($"Lidhja me {model} ne {endpoint} eshte aktive!\n\nModeli u pergjigj.", "Sukses",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lidhja deshtoi:\n{ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            lblTest.Enabled = true;
            lblTest.Text = "Testo lidhjen";
        }
    }
}
