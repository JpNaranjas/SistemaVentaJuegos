namespace TiendaJuegos;

/// <summary>Pantalla de inicio de sesión de Aura Gaming: un botón por perfil.</summary>
public class Form1 : Form
{
    readonly TextBox txtUsuario = new TextBox();
    readonly TextBox txtPassword = new TextBox();
    readonly Button btnAdmin = new Button();
    readonly Button btnOperador = new Button();

    public Form1()
    {
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 11F);
        Text = "Aura Gaming - Inicio de sesión";
        ClientSize = new Size(1100, 940);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.White;

        var morado = Color.FromArgb(106, 27, 154);

        // ---------- Zona central ----------
        var centro = new Panel { Dock = DockStyle.Fill };

        var lblTitulo = new Label
        {
            Text = "AURA GAMING", Dock = DockStyle.Top, Height = 90, TextAlign = ContentAlignment.BottomCenter,
            Font = new Font("Segoe UI Black", 36F, FontStyle.Bold), ForeColor = morado
        };
        var lblSub = new Label
        {
            Text = "Videojuegos y juegos de mesa", Dock = DockStyle.Top, Height = 36, TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 14F, FontStyle.Italic), ForeColor = Color.FromArgb(230, 81, 0)
        };

        var caja = new Panel { Width = 440, Height = 300 };
        caja.Controls.Add(new Label { Text = "Usuario de BD:", AutoSize = true, Left = 0, Top = 0 });
        txtUsuario.SetBounds(0, 26, 440, 30);
        caja.Controls.Add(txtUsuario);
        caja.Controls.Add(new Label { Text = "Contraseña:", AutoSize = true, Left = 0, Top = 70 });
        txtPassword.SetBounds(0, 96, 440, 30);
        txtPassword.UseSystemPasswordChar = true;
        caja.Controls.Add(txtPassword);

        btnAdmin.Text = "Iniciar sesión como ADMINISTRADOR";
        btnAdmin.SetBounds(0, 150, 440, 46);
        btnAdmin.FlatStyle = FlatStyle.Flat;
        btnAdmin.BackColor = morado; btnAdmin.ForeColor = Color.White;
        btnAdmin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnAdmin.Click += (s, e) => Intentar("Administrador", "rol_administrador");

        btnOperador.Text = "Iniciar sesión como OPERADOR";
        btnOperador.SetBounds(0, 206, 440, 46);
        btnOperador.FlatStyle = FlatStyle.Flat;
        btnOperador.BackColor = Color.FromArgb(230, 81, 0); btnOperador.ForeColor = Color.White;
        btnOperador.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnOperador.Click += (s, e) => Intentar("Operador", "rol_operador");

        caja.Controls.Add(btnAdmin);
        caja.Controls.Add(btnOperador);

        centro.Controls.Add(caja);
        centro.Controls.Add(lblSub);
        centro.Controls.Add(lblTitulo);
        centro.Resize += (s, e) => caja.Location = new Point((centro.ClientSize.Width - caja.Width) / 2, lblTitulo.Height + lblSub.Height + 25);

        // ---------- Decoración ----------
        var imgMario = new PictureBox { Dock = DockStyle.Left, Width = 220, SizeMode = PictureBoxSizeMode.Zoom, Image = Recursos.Cargar("mario.jpeg"), BackColor = Color.White };
        var imgUno = new PictureBox { Dock = DockStyle.Right, Width = 240, SizeMode = PictureBoxSizeMode.Zoom, Image = Recursos.Cargar("uno.jpeg"), BackColor = Color.White };
        var imgGta = new CoverPictureBox { Dock = DockStyle.Bottom, Height = 400, Imagen = Recursos.Cargar("gta6.png"), FocoY = 0.1f };

        // El orden importa: el último agregado se acopla primero (GTA ocupa todo el ancho abajo)
        Controls.Add(centro);
        Controls.Add(imgMario);
        Controls.Add(imgUno);
        Controls.Add(imgGta);
    }

    void Intentar(string perfil, string rol)
    {
        if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrEmpty(txtPassword.Text))
        {
            MessageBox.Show("Escribe el usuario y la contraseña.", "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            using (var c = Db.Abrir(txtUsuario.Text.Trim(), txtPassword.Text))
            {
                if (!Db.TieneRol(c, rol))
                {
                    MessageBox.Show("El usuario '" + txtUsuario.Text.Trim() + "' no tiene el rol " + rol + " (perfil " + perfil + ").\n\n" +
                        "Roles detectados en su sesión: " + (string.IsNullOrEmpty(Db.RolesActivos(c)) ? "NINGUNO" : Db.RolesActivos(c)),
                        "Perfil incorrecto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Se actualizan las variables de sesión que usa Form2
            Sesion.Usuario = txtUsuario.Text.Trim();
            Sesion.Password = txtPassword.Text;
            Sesion.Perfil = perfil;

            var f2 = new Form2();
            f2.FormClosed += (s, e) => { txtPassword.Clear(); Show(); };
            Hide();
            f2.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(Db.Mensaje(ex), "No se pudo iniciar sesión", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
