namespace ConverterInstaller;

partial class InstaladorForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblTitulo = new Label();
        lblEstado = new Label();
        btnInstalar = new Button();
        SuspendLayout();
        //
        // lblTitulo
        //
        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitulo.Location = new Point(24, 24);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(320, 21);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "Converter";
        lblTitulo.MaximumSize = new Size(360, 0);
        lblTitulo.AutoEllipsis = true;
        //
        // lblSubtitulo
        //
        lblSubtitulo = new Label();
        lblSubtitulo.AutoSize = true;
        lblSubtitulo.Location = new Point(24, 50);
        lblSubtitulo.MaximumSize = new Size(360, 0);
        lblSubtitulo.Font = new Font("Segoe UI", 9.5F);
        lblSubtitulo.ForeColor = Color.DimGray;
        lblSubtitulo.Text = "Convierte audio y video desde el clic derecho. Un solo archivo: haz doble clic y pulsa Instalar. No necesitas instalar nada más.";
        //
        // lblEstado
        //
        lblEstado.AutoSize = true;
        lblEstado.Location = new Point(24, 100);
        lblEstado.Name = "lblEstado";
        lblEstado.Size = new Size(200, 15);
        lblEstado.TabIndex = 1;
        lblEstado.Text = "Pulsa \"Instalar\" para comenzar.";
        //
        // lblAviso
        //
        lblAviso = new Label();
        lblAviso.AutoSize = true;
        lblAviso.ForeColor = Color.Gray;
        lblAviso.Location = new Point(24, 128);
        lblAviso.MaximumSize = new Size(360, 0);
        lblAviso.Font = new Font("Segoe UI", 8F);
        lblAviso.Text = "Al instalar aceptas los Términos de servicio y la Política de privacidad (en la carpeta del programa).";
        //
        // btnInstalar
        //
        btnInstalar.Font = new Font("Segoe UI", 12F);
        btnInstalar.Location = new Point(24, 168);
        btnInstalar.Name = "btnInstalar";
        btnInstalar.Size = new Size(360, 44);
        btnInstalar.TabIndex = 2;
        btnInstalar.Text = "Instalar";
        btnInstalar.UseVisualStyleBackColor = true;
        btnInstalar.Click += btnInstalar_Click;
        //
        // InstaladorForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(404, 250);
        Controls.Add(lblAviso);
        Controls.Add(btnInstalar);
        Controls.Add(lblEstado);
        Controls.Add(lblSubtitulo);
        Controls.Add(lblTitulo);
        Name = "InstaladorForm";
        Padding = new Padding(20);
        ResumeLayout(false);
        PerformLayout();
    }

    private Label lblTitulo;
    private Label lblSubtitulo;
    private Label lblEstado;
    private Label lblAviso;
    private Button btnInstalar;
}
