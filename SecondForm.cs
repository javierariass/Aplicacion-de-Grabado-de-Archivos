using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace AppForm
{
    public class PagoForm : Form
    {
        private TextBox txtPagoEfectivo;
        private TextBox txtPagoTransferencia;
        private Label lblDiferencia;
        private Button btnSoloCalcular;
        private bool facturaRegistrada;

        private decimal total;
        private DataGridView dgvFacturas;
        private int contadorFacturas;

        public PagoForm(decimal total, DataGridView dgvFacturas, int contadorFacturas)
        {
            this.total = total;
            this.dgvFacturas = dgvFacturas;
            this.contadorFacturas = contadorFacturas;

            Text = "Pago del Cliente";
            Size = new Size(520, 360);
            StartPosition = FormStartPosition.CenterScreen;

            Label lblTotal = new Label
            {
                Text = $"Total a pagar: {total:C}",
                Location = new Point(20, 20),
                Size = new Size(480, 40),
                AutoSize = false
            };

            lblTotal.Font = new Font("Segoe UI", 16, FontStyle.Bold | FontStyle.Italic);
            lblTotal.ForeColor = Color.Black;
            lblTotal.TextAlign = ContentAlignment.MiddleLeft;

            Label lblEfectivo = new Label
            {
                Text = "Pago en efectivo:",
                Location = new Point(20, 80),
                AutoSize = true
            };
            txtPagoEfectivo = new TextBox
            {
                Location = new Point(170, 78),
                Width = 140
            };

            Label lblTransferencia = new Label
            {
                Text = "Pago en transferencia:",
                Location = new Point(20, 125),
                AutoSize = true
            };
            txtPagoTransferencia = new TextBox
            {
                Location = new Point(190, 123),
                Width = 140
            };

            btnSoloCalcular = new Button
            {
                Text = "Calcular Diferencia",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnSoloCalcular.Click += BtnSoloCalcular_Click;

            lblDiferencia = new Label
            {
                Text = "Diferencia: ",
                Location = new Point(20, 175),
                AutoSize = true
            };

            lblDiferencia.Font = new Font("Segoe UI", 10, FontStyle.Bold | FontStyle.Italic); 
            lblDiferencia.ForeColor = Color.Black;

            Controls.Add(lblTotal);
            Controls.Add(lblEfectivo);
            Controls.Add(txtPagoEfectivo);
            Controls.Add(lblTransferencia);
            Controls.Add(txtPagoTransferencia);
            Controls.Add(btnSoloCalcular);
            Controls.Add(lblDiferencia);

            FormClosing += PagoForm_FormClosing;
        }

        private void BtnSoloCalcular_Click(object sender, EventArgs e)
        {
            if (facturaRegistrada)
            {
                return;
            }

            decimal pagoEfectivo = 0, pagoTransferencia = 0;
            decimal.TryParse(txtPagoEfectivo.Text, out pagoEfectivo);
            decimal.TryParse(txtPagoTransferencia.Text, out pagoTransferencia);

            decimal pagoCliente = pagoEfectivo + pagoTransferencia;
            decimal diferencia = pagoCliente - total;
            if (pagoCliente < total)
            {
                MessageBox.Show("Monto insuficiente. La suma de pagos no cubre el total a pagar.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lblDiferencia.Text = $"Total: {total:C} | Pagado: {pagoCliente:C} | Diferencia: {diferencia:C}";

            if (pagoEfectivo > total || (pagoEfectivo + pagoTransferencia) > total)
            {
                pagoEfectivo = total - pagoTransferencia;
                if (pagoEfectivo < 0) pagoEfectivo = 0;
            }

            int totalIndex = -1;
            for (int i = 0; i < dgvFacturas.Rows.Count; i++)
            {
                var valor = dgvFacturas.Rows[i].Cells["NoFactura"].Value;
                if (valor != null && string.Equals(valor.ToString(), "TOTAL", StringComparison.OrdinalIgnoreCase))
                {
                    totalIndex = i;
                    break;
                }
            }

            if (totalIndex >= 0)
            {
                dgvFacturas.Rows.Insert(totalIndex, contadorFacturas, DateTime.Now.ToShortDateString(), total,
                                        pagoEfectivo, pagoTransferencia);
            }
            else
            {
                dgvFacturas.Rows.Add(contadorFacturas, DateTime.Now.ToShortDateString(), total,
                                    pagoEfectivo, pagoTransferencia);
            }

            GuardarFacturaEnBD(contadorFacturas, total, pagoEfectivo, pagoTransferencia);

            contadorFacturas++;
            facturaRegistrada = true;
        }

        private void PagoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (facturaRegistrada)
            {
                DialogResult = DialogResult.OK;
            }
        }


        private void GuardarFacturaEnBD(int noFactura, decimal total, decimal pagoEfectivo, decimal pagoTransferencia)
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
                INSERT INTO Facturas (NoFactura, Fecha, PagoTotal, PagoEfectivo, PagoTransferencia)
                VALUES ($noFactura, $fecha, $pagoTotal, $pagoEfectivo, $pagoTransferencia);
                ";

                command.Parameters.AddWithValue("$noFactura", noFactura);
                command.Parameters.AddWithValue("$fecha", DateTime.Now.ToShortDateString());
                command.Parameters.AddWithValue("$pagoTotal", total);
                command.Parameters.AddWithValue("$pagoEfectivo", pagoEfectivo);
                command.Parameters.AddWithValue("$pagoTransferencia", pagoTransferencia);

                command.ExecuteNonQuery();
            }
        }


        public int GetContador() => contadorFacturas;
    }
}
