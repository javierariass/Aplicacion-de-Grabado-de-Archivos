using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;


namespace AppForm
{
    public partial class MainForm : Form
    {
        private int contadorFacturas = 1;
        public MainForm()
        {
            InitializeComponent();
            
            contadorFacturas = ObtenerUltimoNumeroFactura() + 1;

            // Crear TabControl
            TabControl tabControl = new TabControl { Dock = DockStyle.Fill };

            Panel panelPrecios = new Panel { Dock = DockStyle.Fill };

            // Crear pestañas
            TabPage tab1 = new TabPage("PRECIOS");
            TabPage tab2 = new TabPage("FACTURAS");
            TabPage tab3 = new TabPage("OPCIONES");

            // Crear DataGridViews
            DataGridView dgv1 = new DataGridView { Dock = DockStyle.Top, Height = 400 };
            DataGridView dgv2 = new DataGridView { Dock = DockStyle.Fill };

            // Estilo bonito
            ApplyStyle(dgv1);
            ApplyStyle(dgv2);

            // Agregar tablas a pestañas
            tab1.Controls.Add(dgv1);
            tab2.Controls.Add(dgv2);

            DataGridViewButtonColumn colBoton = new DataGridViewButtonColumn();
            colBoton.Name = "Agregar";
            colBoton.HeaderText = "";
            colBoton.Text = "+";
            colBoton.UseColumnTextForButtonValue = true;
            colBoton.Width = 40;

            dgv1.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && dgv1.Columns[e.ColumnIndex].Name == "Agregar")
                {
                    if (EsFilaTotal(dgv1, e.RowIndex))
                    {
                        return;
                    }

                    var celdaCantidad = dgv1.Rows[e.RowIndex].Cells["Cantidad"];

                    int cantidadActual = 0;
                    if (celdaCantidad.Value != null)
                        int.TryParse(celdaCantidad.Value.ToString(), out cantidadActual);

                    celdaCantidad.Value = cantidadActual + 1;
                    ActualizarFilaTotal(dgv1, e.RowIndex);
                    ActualizarTotales(dgv1);
                }
            };

            dgv1.CellPainting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && dgv1.Columns[e.ColumnIndex].Name == "Agregar" && e.RowIndex >= 0)
                {
                    e.Graphics.FillRectangle(new SolidBrush(dgv1.DefaultCellStyle.BackColor), e.CellBounds);

                    if (EsFilaTotal(dgv1, e.RowIndex))
                    {
                        e.Handled = true;
                        return;
                    }

                    using (Brush b = new SolidBrush(Color.LightBlue))
                    {
                        int size = Math.Min(e.CellBounds.Width, e.CellBounds.Height) - 6;
                        int x = e.CellBounds.Left + (e.CellBounds.Width - size) / 2;
                        int y = e.CellBounds.Top + (e.CellBounds.Height - size) / 2;

                        e.Graphics.FillEllipse(b, x, y, size, size);

                        using (Font f = new Font("Segoe UI", 10, FontStyle.Bold))
                        {
                            TextRenderer.DrawText(e.Graphics, "+", f,
                                new Rectangle(x, y, size, size),
                                Color.White,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }

                    e.Handled = true; 
                }
            };

            
            // Agregar columnas al DataGridView de Precios
            dgv1.Columns.Add("Informacion", "INFORMACION");
            dgv1.Columns.Add("Precio", "PRECIO");
            dgv1.Columns.Add("Cantidad", "CANT.");
            dgv1.Columns.Add("PTotal", "P. TOTAL");
            dgv1.Columns.Add(colBoton);

            dgv1.Rows.Add("Animado (corto variado)", 2);
            dgv1.Rows.Add("Serie Animada", 3);
            dgv1.Rows.Add("Pelicula Animada", 5);
            dgv1.Rows.Add("Show latino", 3);
            dgv1.Rows.Add("Novela", 3);
            dgv1.Rows.Add("Serie/Dorama", 4);
            dgv1.Rows.Add("Documental", 3);
            dgv1.Rows.Add("Concurso/Reality", 7);
            dgv1.Rows.Add("Pelicula (AVI/MPG/VOB)", 7);
            dgv1.Rows.Add("Pelicula (HD/MP4)", 20);
            dgv1.Rows.Add("Deporte", 10);
            dgv1.Rows.Add("Musica x GB (Audio-Video)", 100);
            dgv1.Rows.Add("Juego (Detective)", 100);
            dgv1.Rows.Add("Juego (PC)", 200);
            dgv1.Rows.Add("Aplicaciones (PC/APK)", 100);
            dgv1.Rows.Add("Windows/Pak Drivers", 200);
            dgv1.Rows.Add("Booteable USB", 500);
            dgv1.Rows.Add("Conversion MP4 x TV", 50);
            dgv1.Rows.Add("Paquete Semanal", 500);

            dgv1.AllowUserToAddRows = false;
            dgv1.Columns["Informacion"].ReadOnly = true;
            dgv1.Columns["Precio"].ReadOnly = true;
            dgv1.Columns["PTotal"].ReadOnly = true;
            dgv1.RowHeadersVisible = false;
            dgv1.AllowUserToResizeColumns = false;
            dgv1.AllowUserToResizeRows = false;
            dgv1.ScrollBars = ScrollBars.Vertical;

            dgv2.AllowUserToAddRows = false;
            dgv2.ReadOnly = true;
            dgv2.AllowUserToResizeColumns = false;
            dgv2.AllowUserToResizeRows = false;
            dgv2.ScrollBars = ScrollBars.Vertical;

            dgv1.Columns["Informacion"].Width = 270;
            dgv1.Columns["Precio"].Width = 90;
            dgv1.Columns["Cantidad"].Width = 90;
            dgv1.Columns["PTotal"].Width = 90;

            dgv1.Dock = DockStyle.Fill;

            // Agregar columnas al DataGridView de Facturas
            dgv2.Columns.Add("NoFactura", "NO.");
            dgv2.Columns.Add("Fecha", "FECHA");
            dgv2.Columns.Add("PagoTotal", "PAGO TOTAL");
            dgv2.Columns.Add("PagoEfectivo", "EFECTIVO");
            dgv2.Columns.Add("PagoTransferencia", "TRANSFERENCIA");

            dgv2.Columns["NoFactura"].Width = 80;
            dgv2.Columns["Fecha"].Width = 110;
            dgv2.Columns["PagoTotal"].Width = 120;
            dgv2.Columns["PagoEfectivo"].Width = 110;
            dgv2.Columns["PagoTransferencia"].Width = 120;
            dgv2.RowHeadersVisible = true;
            dgv2.RowHeadersWidth = 40;

            Button btnFacturar = new Button
            {
                Text = "Facturar",
                Width = 200,
                Height = 50,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Fill
            };
            btnFacturar.Click += (s, e) =>
            {
                bool hayCantidad = false;
                foreach (DataGridViewRow row in dgv1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        if (EsFilaTotal(dgv1, row.Index))
                        {
                            continue;
                        }
                        var valor = row.Cells["Cantidad"].Value;
                        if (valor != null && !string.IsNullOrWhiteSpace(valor.ToString()))
                        {
                            hayCantidad = true;
                            break; 
                        }
                    }
                }

                if (!hayCantidad)
                {
                    MessageBox.Show("No hay datos que procesar en la columna Cantidad.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; 
                }
                
                decimal suma = 0;
                foreach (DataGridViewRow row in dgv1.Rows)
                {
                    if (EsFilaTotal(dgv1, row.Index))
                    {
                        continue;
                    }
                    if (row.Cells["Precio"].Value != null &&
                        decimal.TryParse(row.Cells["Precio"].Value.ToString(), out decimal precio))
                    {
                        int cantidad = 0;

                        if (row.Cells["Cantidad"].Value != null)
                        {
                            if (!int.TryParse(row.Cells["Cantidad"].Value.ToString(), out cantidad))
                            {
                                MessageBox.Show("Error: La columna 'Cantidad' solo admite números enteros.",
                                                "Dato inválido",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);
                                return; 
                            }
                        }

                        suma += precio * cantidad;
                    }
                }

                // Abrir ventana de pago con el total
                PagoForm pagoForm = new PagoForm(suma, dgv2, contadorFacturas);
                if (pagoForm.ShowDialog() == DialogResult.OK)
                {
                    contadorFacturas = pagoForm.GetContador();
                    AsegurarFilaTotalFacturas(dgv2);
                    ActualizarTotalesFacturas(dgv2);
                    tabControl.SelectedTab = tab2;
                }
                foreach (DataGridViewRow row in dgv1.Rows)
                {
                    if (!EsFilaTotal(dgv1, row.Index))
                    {
                        row.Cells["Cantidad"].Value = null;
                        row.Cells["PTotal"].Value = null;
                    }
                }
                ActualizarTotales(dgv1);
            };

            // Recalcular P. TOTAL al editar Cant.
            dgv1.CellEndEdit += (s, e) =>
            {
                if (e.ColumnIndex == dgv1.Columns["Cantidad"].Index && e.RowIndex >= 0)
                {
                    if (EsFilaTotal(dgv1, e.RowIndex))
                    {
                        return;
                    }

                    var celda = dgv1.Rows[e.RowIndex].Cells["Cantidad"];
                    if (celda.Value != null && !int.TryParse(celda.Value.ToString(), out _))
                    {
                        MessageBox.Show("Error: La columna 'Cantidad' solo admite numeros enteros.",
                                        "Dato invalido",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                        celda.Value = null;
                    }

                    ActualizarFilaTotal(dgv1, e.RowIndex);
                    ActualizarTotales(dgv1);
                }
            };

            // Panel para los botones
            Panel panelBotones = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            panelBotones.Controls.Add(btnFacturar);

            Button btnLimpiarFacturas = new Button
            {
                Text = "LIMPIAR FACTURA",
                Width = 200,
                Height = 50,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Bottom
            };
            btnLimpiarFacturas.Click += (s, e) =>
            {
                if (!SolicitarContrasena("2020"))
                {
                    return;
                }

                LimpiarFacturasDB();
                CargarFacturas(dgv2);
            };

            // Agregar controles a pestañas
            tab1.Controls.Add(dgv1);
            tab1.Controls.Add(panelBotones);
            tab1.Controls.Add(panelPrecios);
            tab2.Controls.Add(dgv2);
            tab2.Controls.Add(btnLimpiarFacturas);


            // Agregar pestañas al TabControl
            tabControl.TabPages.Add(tab1);
            tabControl.TabPages.Add(tab2);

            // Agregar al formulario
            Controls.Add(tabControl);

            // Crear MenuStrip después de agregar TabControl
            MenuStrip menuStrip = new MenuStrip();
            ToolStripMenuItem menuAyuda = new ToolStripMenuItem("Ayuda");
            ToolStripMenuItem itemAcercaDe = new ToolStripMenuItem("Acerca de");
            itemAcercaDe.Click += (s, e) => BtnAcercaDe_Click(s, e);
            menuAyuda.DropDownItems.Add(itemAcercaDe);
            menuStrip.Items.Add(menuAyuda);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;

            Text = "Aplicacion de Grabacion";
            int anchoTablaPrecios = dgv1.Columns["Informacion"].Width + dgv1.Columns["Precio"].Width +
                                   dgv1.Columns["Cantidad"].Width + dgv1.Columns["PTotal"].Width +
                                   colBoton.Width + 20;
            int anchoTablaFacturas = dgv2.RowHeadersWidth + dgv2.Columns["NoFactura"].Width +
                                    dgv2.Columns["Fecha"].Width + dgv2.Columns["PagoTotal"].Width +
                                    dgv2.Columns["PagoEfectivo"].Width + dgv2.Columns["PagoTransferencia"].Width + 20;
            int anchoFinal = Math.Max(anchoTablaPrecios, anchoTablaFacturas);
            ClientSize = new Size(anchoFinal + 20, 650);
            StartPosition = FormStartPosition.Manual;

            // Posicionar en la esquina derecha de la pantalla
            Screen screen = Screen.PrimaryScreen;
            int x = screen.WorkingArea.Right - Width - 10;
            int y = screen.WorkingArea.Top + 10;
            Location = new Point(x, y);

            PrepararFilasColores(dgv1);
            AgregarFilaTotal(dgv1);
            ActualizarTotales(dgv1);
            CargarFacturas(dgv2);

            int filasPrecios = dgv1.Rows.Count;
            int alturaTablaPrecios = dgv1.ColumnHeadersHeight + (dgv1.RowTemplate.Height * filasPrecios) + 2;
            int alturaExtra = panelBotones.Height + menuStrip.Height + tabControl.ItemSize.Height + 40;
            ClientSize = new Size(ClientSize.Width, alturaTablaPrecios + alturaExtra);
        }

        private void BtnAcercaDe_Click(object sender, EventArgs e)
        {
            AcercaDeForm acercaDe = new AcercaDeForm();
            acercaDe.ShowDialog();
        }

        private void ApplyStyle(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.WhiteSmoke;
            dgv.DefaultCellStyle.BackColor = Color.AliceBlue;
            dgv.DefaultCellStyle.ForeColor = Color.DarkSlateGray;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.SteelBlue;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.EnableHeadersVisualStyles = false;
        }

        private void PrepararFilasColores(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                string nombre = Convert.ToString(row.Cells["Informacion"].Value);
                if (string.Equals(nombre, "Show latino", StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.BackColor = Color.LightSkyBlue;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
                else if (string.Equals(nombre, "Novela", StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.BackColor = Color.DeepSkyBlue;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
                else if (string.Equals(nombre, "Serie/Dorama", StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.BackColor = Color.Gold;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
                else if (string.Equals(nombre, "Concurso/Reality", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(nombre, "Pelicula (AVI/MPG/VOB)", StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.BackColor = Color.Lime;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
                else if (string.Equals(nombre, "Paquete Semanal", StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.BackColor = Color.LightPink;
                    row.DefaultCellStyle.ForeColor = Color.Maroon;
                }
                else if (string.Equals(nombre, "Pelicula Animada", StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.BackColor = Color.Orange;
                    row.DefaultCellStyle.ForeColor = Color.Maroon;
                }
                else if (string.Equals(nombre, "Conversion MP4 x TV", StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
            }
        }

        private void AgregarFilaTotal(DataGridView dgv)
        {
            int index = dgv.Rows.Add("TOTAL", null, null, null);
            DataGridViewRow totalRow = dgv.Rows[index];
            totalRow.ReadOnly = true;
            totalRow.DefaultCellStyle.BackColor = Color.AliceBlue;
            totalRow.DefaultCellStyle.ForeColor = Color.Black;
            totalRow.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        private bool EsFilaTotal(DataGridView dgv, int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgv.Rows.Count)
            {
                return false;
            }

            var valor = dgv.Rows[rowIndex].Cells["Informacion"].Value;
            return valor != null && string.Equals(valor.ToString(), "TOTAL", StringComparison.OrdinalIgnoreCase);
        }

        private void ActualizarFilaTotal(DataGridView dgv, int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgv.Rows.Count)
            {
                return;
            }

            DataGridViewRow row = dgv.Rows[rowIndex];
            if (row.IsNewRow || EsFilaTotal(dgv, rowIndex))
            {
                return;
            }

            if (!decimal.TryParse(Convert.ToString(row.Cells["Precio"].Value), out decimal precio))
            {
                return;
            }

            int cantidad = 0;
            if (row.Cells["Cantidad"].Value != null)
            {
                int.TryParse(row.Cells["Cantidad"].Value.ToString(), out cantidad);
            }

            row.Cells["PTotal"].Value = (precio * cantidad == 0) ? null : precio * cantidad;
        }

        private void ActualizarTotales(DataGridView dgv)
        {
            int totalCantidad = 0;
            decimal totalPago = 0;

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow || EsFilaTotal(dgv, row.Index))
                {
                    continue;
                }

                int cantidad = 0;
                if (row.Cells["Cantidad"].Value != null)
                {
                    int.TryParse(row.Cells["Cantidad"].Value.ToString(), out cantidad);
                }

                if (decimal.TryParse(Convert.ToString(row.Cells["Precio"].Value), out decimal precio))
                {
                    totalCantidad += cantidad;
                    totalPago += precio * cantidad;
                }
            }

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (EsFilaTotal(dgv, row.Index))
                {
                    row.Cells["Cantidad"].Value = totalCantidad == 0 ? null : totalCantidad;
                    row.Cells["PTotal"].Value = totalPago == 0 ? null : totalPago;
                }
            }
        }

        private void CrearBaseDatos()
        {
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            string parentFolder = Directory.GetParent(exeFolder).FullName;
            string carpetaBD = Path.Combine(parentFolder, "Base de Datos");
            if (!Directory.Exists(carpetaBD))
            {
                Directory.CreateDirectory(carpetaBD);
            }

            string rutaDB = Path.Combine(carpetaBD, "facturas.db");
            using (var connection = new SqliteConnection($"Data Source={rutaDB}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                CREATE TABLE IF NOT EXISTS Facturas (
                    NoFactura INTEGER PRIMARY KEY,
                    Fecha TEXT,
                    PagoTotal REAL,
                    PagoEfectivo REAL,
                    PagoTransferencia REAL
                );
                ";
                command.ExecuteNonQuery();
            }
        }

        private void CargarFacturas(DataGridView dgv)
        {
            dgv.Rows.Clear();
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            string parentFolder = Directory.GetParent(exeFolder).FullName;
            string carpetaBD = Path.Combine(parentFolder, "Base de Datos");
            if (!Directory.Exists(carpetaBD))
            {
                Directory.CreateDirectory(carpetaBD);
            }
            string rutaDB = Path.Combine(carpetaBD, "facturas.db");
            using (var connection = new SqliteConnection($"Data Source={rutaDB}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Facturas";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dgv.Rows.Add(reader.GetInt32(0),   // NoFactura
                                    reader.GetString(1),   // Fecha
                                    reader.GetDecimal(2),  // PagoTotal
                                    reader.GetDecimal(3),  // PagoEfectivo
                                    reader.GetDecimal(4)); // PagoTransferencia
                    }
                }
            }

            AsegurarFilaTotalFacturas(dgv);
            ActualizarTotalesFacturas(dgv);
        }

        private void AsegurarFilaTotalFacturas(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (EsFilaTotalFacturas(row))
                {
                    return;
                }
            }

            int index = dgv.Rows.Add("TOTAL", null, null, null, null);
            DataGridViewRow totalRow = dgv.Rows[index];
            totalRow.ReadOnly = true;
            totalRow.DefaultCellStyle.BackColor = Color.LightGreen;
            totalRow.DefaultCellStyle.ForeColor = Color.Black;
            totalRow.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        private bool EsFilaTotalFacturas(DataGridViewRow row)
        {
            var valor = row.Cells["NoFactura"].Value;
            return valor != null && string.Equals(valor.ToString(), "TOTAL", StringComparison.OrdinalIgnoreCase);
        }

        private void ActualizarTotalesFacturas(DataGridView dgv)
        {
            decimal totalPago = 0;
            decimal totalEfectivo = 0;
            decimal totalTransferencia = 0;

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow || EsFilaTotalFacturas(row))
                {
                    continue;
                }

                if (decimal.TryParse(Convert.ToString(row.Cells["PagoTotal"].Value), out decimal pagoTotal))
                {
                    totalPago += pagoTotal;
                }

                if (decimal.TryParse(Convert.ToString(row.Cells["PagoEfectivo"].Value), out decimal pagoEfectivo))
                {
                    totalEfectivo += pagoEfectivo;
                }

                if (decimal.TryParse(Convert.ToString(row.Cells["PagoTransferencia"].Value), out decimal pagoTransferencia))
                {
                    totalTransferencia += pagoTransferencia;
                }
            }

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (EsFilaTotalFacturas(row))
                {
                    row.Cells["Fecha"].Value = DateTime.Now.ToShortDateString();
                    row.Cells["PagoTotal"].Value = totalPago == 0 ? null : totalPago;
                    row.Cells["PagoEfectivo"].Value = totalEfectivo == 0 ? null : totalEfectivo;
                    row.Cells["PagoTransferencia"].Value = totalTransferencia == 0 ? null : totalTransferencia;
                }
            }
        }

        private void LimpiarFacturasDB()
        {
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            string parentFolder = Directory.GetParent(exeFolder).FullName;
            string carpetaBD = Path.Combine(parentFolder, "Base de Datos");
            if (!Directory.Exists(carpetaBD))
            {
                Directory.CreateDirectory(carpetaBD);
            }

            string rutaDB = Path.Combine(carpetaBD, "facturas.db");
            using (var connection = new SqliteConnection($"Data Source={rutaDB}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Facturas";
                command.ExecuteNonQuery();
            }
        }

        private bool SolicitarContrasena(string claveEsperada)
        {
            using (Form dialogo = new Form())
            using (Label lbl = new Label())
            using (TextBox txt = new TextBox())
            using (Button btnAceptar = new Button())
            {
                dialogo.Text = "Contrasena";
                dialogo.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialogo.ClientSize = new Size(420, 230);
                dialogo.StartPosition = FormStartPosition.CenterParent;
                dialogo.MaximizeBox = false;
                dialogo.MinimizeBox = false;

                lbl.Text = "Introduzca su Contrasena";
                lbl.Font = new Font("Segoe UI", 12, FontStyle.Regular);
                lbl.AutoSize = false;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.SetBounds(20, 30, 380, 30);

                txt.PasswordChar = '*';
                txt.SetBounds(90, 80, 240, 28);

                btnAceptar.Text = "Aceptar";
                btnAceptar.SetBounds(150, 130, 120, 40);
                btnAceptar.DialogResult = DialogResult.OK;

                dialogo.Controls.AddRange(new Control[] { lbl, txt, btnAceptar });
                dialogo.AcceptButton = btnAceptar;

                if (dialogo.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }

                if (!string.Equals(txt.Text, claveEsperada, StringComparison.Ordinal))
                {
                    MessageBox.Show("Contrasena incorrecta.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
        }

        private int ObtenerUltimoNumeroFactura()
        {
            try
            {
                string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
                string parentFolder = Directory.GetParent(exeFolder).FullName;
                string carpetaBD = Path.Combine(parentFolder, "Base de Datos");
                if (!Directory.Exists(carpetaBD))
                {
                    Directory.CreateDirectory(carpetaBD);
                }

                string rutaDB = Path.Combine(carpetaBD, "facturas.db");
                using (var connection = new SqliteConnection($"Data Source={rutaDB}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT IFNULL(MAX(NoFactura), 0) FROM Facturas";

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch
            {
                CrearBaseDatos();
                return 0;
            }        
        }

    }
}
