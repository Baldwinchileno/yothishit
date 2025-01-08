﻿using System;
using System.Drawing;
using System.Windows.Forms;
using AdminSERMAC.Core.Interfaces;
using AdminSERMAC.Exceptions;
using AdminSERMAC.Models;
using AdminSERMAC.Services;
using Microsoft.Extensions.Logging;

namespace AdminSERMAC.Forms
{
    public class ClientesForm : Form
    {
        private readonly IClienteService _clienteService;

        // Controles existentes
        private ComboBox clientesComboBox;
        private Label deudaLabel;
        private Label seleccionarClienteLabel;
        private DataGridView ventasDataGridView;

        // Nuevos controles para CRUD
        private TextBox rutTextBox;
        private TextBox nombreTextBox;
        private TextBox direccionTextBox;
        private TextBox giroTextBox;
        private Button agregarButton;
        private Button actualizarButton;
        private Button eliminarButton;
        private Button limpiarButton;
        private GroupBox clienteGroupBox;

        // Controles para abonos
        private GroupBox grupoAbono;
        private TextBox montoAbonoTextBox;
        private Button registrarAbonoButton;

        private Button modificarClienteButton;
        private DataGridView clientesDataGridView;
        private readonly SQLiteService sqliteService;
        private readonly ILogger<ClientesForm> _logger;

        public ClientesForm(SQLiteService sqliteService, ILogger<ClientesForm> logger)
        {
            this.sqliteService = sqliteService ?? throw new ArgumentNullException(nameof(sqliteService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Gestión de Clientes";
            this.Width = 1000;
            this.Height = 800;

            // GroupBox para datos del cliente
            clienteGroupBox = new GroupBox()
            {
                Text = "Datos del Cliente",
                Top = 20,
                Left = 50,
                Width = 400,
                Height = 200
            };

            // RUT
            var rutLabel = new Label()
            {
                Text = "RUT:",
                Top = 30,
                Left = 20,
                Width = 100
            };
            rutTextBox = new TextBox()
            {
                Top = 30,
                Left = 130,
                Width = 200
            };
            rutTextBox.Leave += ValidarRUT;

            // Nombre
            var nombreLabel = new Label()
            {
                Text = "Nombre:",
                Top = 60,
                Left = 20,
                Width = 100
            };
            nombreTextBox = new TextBox()
            {
                Top = 60,
                Left = 130,
                Width = 200
            };

            // Dirección
            var direccionLabel = new Label()
            {
                Text = "Dirección:",
                Top = 90,
                Left = 20,
                Width = 100
            };
            direccionTextBox = new TextBox()
            {
                Top = 90,
                Left = 130,
                Width = 200
            };

            // Giro
            var giroLabel = new Label()
            {
                Text = "Giro:",
                Top = 120,
                Left = 20,
                Width = 100
            };
            giroTextBox = new TextBox()
            {
                Top = 120,
                Left = 130,
                Width = 200
            };

            // Botones de acción
            agregarButton = new Button()
            {
                Text = "Agregar Cliente",
                Top = 160,
                Left = 20,
                Width = 120
            };
            agregarButton.Click += AgregarButton_Click;

            actualizarButton = new Button()
            {
                Text = "Actualizar",
                Top = 160,
                Left = 150,
                Width = 120,
                Enabled = false
            };
            actualizarButton.Click += ActualizarButton_Click;

            eliminarButton = new Button()
            {
                Text = "Eliminar",
                Top = 160,
                Left = 280,
                Width = 100,
                Enabled = false
            };
            eliminarButton.Click += EliminarButton_Click;

            // Agregar controles al GroupBox
            clienteGroupBox.Controls.AddRange(new Control[] {
               rutLabel, rutTextBox,
               nombreLabel, nombreTextBox,
               direccionLabel, direccionTextBox,
               giroLabel, giroTextBox,
               agregarButton, actualizarButton, eliminarButton
           });

            modificarClienteButton = new Button
            {
                Text = "Modificar Cliente",
                Location = new Point(20, 20),
                Width = 150
            };
            modificarClienteButton.Click += ModificarClienteButton_Click;

            // Agregar el botón al formulario
            this.Controls.Add(modificarClienteButton);

            // Grupo de abono
            grupoAbono = new GroupBox()
            {
                Text = "Registrar Abono",
                Top = 240,
                Left = 500,
                Width = 300,
                Height = 120
            };

            var montoAbonoLabel = new Label()
            {
                Text = "Monto Abono ($):",
                Top = 30,
                Left = 20,
                Width = 100
            };

            montoAbonoTextBox = new TextBox()
            {
                Top = 30,
                Left = 120,
                Width = 150
            };

            registrarAbonoButton = new Button()
            {
                Text = "Registrar Abono",
                Top = 70,
                Left = 120,
                Width = 150,
                Enabled = false
            };

            grupoAbono.Controls.AddRange(new Control[] {
               montoAbonoLabel,
               montoAbonoTextBox,
               registrarAbonoButton
           });

            // ComboBox de clientes existentes
            seleccionarClienteLabel = new Label()
            {
                Text = "Seleccionar Cliente Existente:",
                Top = 380,
                Left = 50,
                Width = 200
            };

            clientesComboBox = new ComboBox()
            {
                Top = 410,
                Left = 50,
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            clientesComboBox.SelectedIndexChanged += ClientesComboBox_SelectedIndexChanged;

            // Label de deuda
            deudaLabel = new Label()
            {
                Top = 450,
                Left = 50,
                Width = 400,
                Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold)
            };

            // DataGridView de ventas y deudas
            ventasDataGridView = new DataGridView()
            {
                Top = 490,
                Left = 50,
                Width = 900,
                Height = 250,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Configurar columnas del DataGridView
            ventasDataGridView.Columns.Add("FechaVenta", "Fecha");
            ventasDataGridView.Columns.Add("NumeroGuia", "N° Guía");
            ventasDataGridView.Columns.Add("Descripcion", "Descripción");
            ventasDataGridView.Columns.Add("KilosNeto", "Kilos");
            ventasDataGridView.Columns.Add("Monto", "Monto");
            ventasDataGridView.Columns.Add("Estado", "Estado");
            ventasDataGridView.Columns.Add("DiasMora", "Días Mora");
            // Configuración adicional del DataGridView
            ventasDataGridView.DefaultCellStyle.SelectionBackColor = Color.LightSteelBlue;
            ventasDataGridView.DefaultCellStyle.SelectionForeColor = Color.Black;
            ventasDataGridView.RowHeadersVisible = false;
            ventasDataGridView.AllowUserToResizeRows = false;
            ventasDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            ventasDataGridView.Columns.Clear();
            ventasDataGridView.Columns.AddRange(new DataGridViewColumn[] {
    new DataGridViewTextBoxColumn
    {
        Name = "FechaVenta",
        HeaderText = "Fecha",
        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
    },
    new DataGridViewTextBoxColumn
    {
        Name = "NumeroGuia",
        HeaderText = "N° Guía",
        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
    },
    new DataGridViewTextBoxColumn
    {
        Name = "Descripcion",
        HeaderText = "Descripción",
        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
    },
    new DataGridViewTextBoxColumn
    {
        Name = "KilosNeto",
        HeaderText = "Kilos",
        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
    },
    new DataGridViewTextBoxColumn
    {
        Name = "Total",
        HeaderText = "Monto",
        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
    },
    new DataGridViewTextBoxColumn
    {
        Name = "Estado",
        HeaderText = "Estado",
        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
    },
    new DataGridViewTextBoxColumn
    {
        Name = "DiasMora",
        HeaderText = "Días Mora",
        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
    }
            });

            // Inicializar clientesDataGridView
            clientesDataGridView = new DataGridView()
            {
                Top = 550,
                Left = 50,
                Width = 900,
                Height = 200,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Agregar controles al formulario
            this.Controls.AddRange(new Control[] {
               clienteGroupBox,
               grupoAbono,
               seleccionarClienteLabel,
               clientesComboBox,
               deudaLabel,
               ventasDataGridView,
               clientesDataGridView
           });

            registrarAbonoButton.Click += RegistrarAbonoButton_Click;
        }

        private void ValidarRUT(object sender, EventArgs e)
        {
            string rut = rutTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(rut))
            {
                try
                {
                    if (!ValidarFormatoRUT(rut))
                    {
                        MessageBox.Show("El formato del RUT no es válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        rutTextBox.Focus();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ModificarClienteButton_Click(object sender, EventArgs e)
        {
            if (clientesDataGridView.SelectedRows.Count > 0)
            {
                var selectedRow = clientesDataGridView.SelectedRows[0];
                var cliente = new Cliente
                {
                    RUT = selectedRow.Cells["RUT"].Value?.ToString(),
                    Nombre = selectedRow.Cells["Nombre"].Value?.ToString(),
                    Direccion = selectedRow.Cells["Direccion"].Value?.ToString(),
                    Giro = selectedRow.Cells["Giro"].Value?.ToString(),
                    Deuda = Convert.ToDouble(selectedRow.Cells["Deuda"].Value)
                };

                var modificarClienteForm = new ModificarClienteForm(cliente, sqliteService, _logger);
                modificarClienteForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Seleccione un cliente para modificar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(rutTextBox.Text))
            {
                MessageBox.Show("El RUT es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                rutTextBox.Focus();
                return false;
            }

            if (!ValidarFormatoRUT(rutTextBox.Text.Trim()))
            {
                MessageBox.Show("El formato del RUT no es válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                rutTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(nombreTextBox.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nombreTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(direccionTextBox.Text))
            {
                MessageBox.Show("La dirección es obligatoria.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                direccionTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(giroTextBox.Text))
            {
                MessageBox.Show("El giro es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                giroTextBox.Focus();
                return false;
            }

            return true;
        }

        private bool ValidarFormatoRUT(string rut)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(rut, @"^\d{1,2}\.\d{3}\.\d{3}[-][0-9kK]{1}$");
        }

        private void AgregarButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarCampos()) return;

                var nuevoCliente = new Cliente
                {
                    RUT = rutTextBox.Text.Trim(),
                    Nombre = nombreTextBox.Text.Trim(),
                    Direccion = direccionTextBox.Text.Trim(),
                    Giro = giroTextBox.Text.Trim(),
                    Deuda = 0
                };

                _clienteService.AgregarCliente(nuevoCliente);
                MessageBox.Show("Cliente agregado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LimpiarCampos();
                LoadClientes();
            }
            catch (ClienteDuplicadoException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                rutTextBox.Focus();
            }
            catch (ClienteValidationException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado al agregar el cliente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarCampos()) return;

                var clienteActualizado = new Cliente
                {
                    RUT = rutTextBox.Text.Trim(),
                    Nombre = nombreTextBox.Text.Trim(),
                    Direccion = direccionTextBox.Text.Trim(),
                    Giro = giroTextBox.Text.Trim()
                };

                _clienteService.ActualizarCliente(clienteActualizado);
                MessageBox.Show("Cliente actualizado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LimpiarCampos();
                LoadClientes();
            }
            catch (ClienteNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ClienteValidationException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado al actualizar el cliente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EliminarButton_Click(object sender, EventArgs e)
        {
            try
            {
                string rut = rutTextBox.Text.Trim();
                if (string.IsNullOrEmpty(rut))
                {
                    MessageBox.Show("Seleccione un cliente para eliminar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var resultado = MessageBox.Show(
                    "¿Está seguro que desea eliminar este cliente?\nEsta acción no se puede deshacer.",
                    "Confirmar Eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (resultado == DialogResult.Yes)
                {
                    _clienteService.EliminarCliente(rut);
                    MessageBox.Show("Cliente eliminado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LimpiarCampos();
                    LoadClientes();
                }
            }
            catch (ClienteConVentasException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ClienteNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado al eliminar el cliente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarInformacionCliente(string rut)
        {
            try
            {
                var deudaTotal = _clienteService.CalcularDeudaTotal(rut);
                deudaLabel.Text = $"Deuda Total: ${deudaTotal:N0}";
                CargarVentasCliente(rut);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar información del cliente: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RegistrarAbonoButton_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(montoAbonoTextBox.Text, out double montoAbono) || montoAbono <= 0)
            {
                MessageBox.Show("Ingrese un monto válido mayor a cero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cliente = (Cliente)clientesComboBox.SelectedItem;
            if (cliente == null)
            {
                MessageBox.Show("Por favor seleccione un cliente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Verificamos primero la deuda actual
                var deudaActual = _clienteService.CalcularDeudaTotal(cliente.RUT);
                if (montoAbono > deudaActual)
                {
                    MessageBox.Show($"El monto del abono (${montoAbono:N0}) no puede ser mayor que la deuda actual (${deudaActual:N0}).",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // El monto se pasa como negativo porque es un abono (reduce la deuda)
                double montoNegativo = -montoAbono;
                _clienteService.ActualizarDeudaCliente(cliente.RUT, montoNegativo);

                MessageBox.Show($"Abono por ${montoAbono:N0} registrado exitosamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Limpiar el campo de abono
                montoAbonoTextBox.Clear();

                // Actualizar la visualización de la deuda
                ActualizarInformacionCliente(cliente.RUT);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar abono: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            rutTextBox.Text = string.Empty;
            nombreTextBox.Text = string.Empty;
            direccionTextBox.Text = string.Empty;
            giroTextBox.Text = string.Empty;

            actualizarButton.Enabled = false;
            eliminarButton.Enabled = false;
            agregarButton.Enabled = true;
            rutTextBox.Enabled = true;
            registrarAbonoButton.Enabled = false;

            clientesComboBox.SelectedIndex = -1;
            deudaLabel.Text = string.Empty;
            ventasDataGridView.Rows.Clear();
            montoAbonoTextBox.Clear();
        }

        private void LoadClientes()
        {
            try
            {
                clientesComboBox.Items.Clear();
                var clientes = _clienteService.ObtenerTodosLosClientes();

                foreach (var cliente in clientes)
                {
                    clientesComboBox.Items.Add(cliente);
                }

                clientesComboBox.DisplayMember = "Nombre";
                clientesComboBox.ValueMember = "RUT";
            }
            catch (ClienteException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado al cargar los clientes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClientesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (clientesComboBox.SelectedItem == null) return;

            try
            {
                var cliente = (Cliente)clientesComboBox.SelectedItem;

                rutTextBox.Text = cliente.RUT;
                nombreTextBox.Text = cliente.Nombre;
                direccionTextBox.Text = cliente.Direccion;
                giroTextBox.Text = cliente.Giro;

                rutTextBox.Enabled = false;
                agregarButton.Enabled = false;
                actualizarButton.Enabled = true;
                eliminarButton.Enabled = true;
                registrarAbonoButton.Enabled = true;

                var deudaTotal = _clienteService.CalcularDeudaTotal(cliente.RUT);
                deudaLabel.Text = $"Deuda Total: ${deudaTotal:N0}";

                CargarVentasCliente(cliente.RUT);
            }
            catch (ClienteException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado al cargar los datos del cliente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarVentasCliente(string rut)
        {
            try
            {
                // Verifica si el RUT está vacío o nulo
                if (string.IsNullOrWhiteSpace(rut))
                {
                    MessageBox.Show("El RUT del cliente no puede estar vacío.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Limpia el DataGridView antes de cargar las ventas
                ventasDataGridView.Rows.Clear();

                // Intenta obtener las ventas del cliente
                var ventas = _clienteService.ObtenerVentasCliente(rut);
                if (ventas == null || !ventas.Any())
                {
                    MessageBox.Show($"No se encontraron ventas asociadas al cliente con RUT {rut}.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Procesa cada venta y agrega filas al DataGridView
                foreach (var venta in ventas)
                {
                    // Calcula días de mora
                    int diasMora = (DateTime.Now - DateTime.Parse(venta.FechaVenta)).Days;

                    // Determina el estado de la venta
                    string estado = venta.PagadoConCredito == 1 ? "Pendiente" : "Pagado";

                    // Agrega una fila al DataGridView
                    ventasDataGridView.Rows.Add(
                        DateTime.Parse(venta.FechaVenta).ToString("dd/MM/yyyy"),
                        venta.NumeroGuia,
                        venta.Descripcion,
                        $"{venta.KilosNeto:N2}",
                        $"${venta.Total:N0}",
                        estado,
                        venta.PagadoConCredito == 1 ? diasMora.ToString() : "-"
                    );

                    // Cambia el color de la fila según el estado
                    var rowObj = ventasDataGridView.Rows[ventasDataGridView.Rows.Count - 1];
                    if (venta.PagadoConCredito == 1)
                    {
                        if (diasMora > 30)
                            rowObj.DefaultCellStyle.BackColor = Color.LightCoral;
                        else if (diasMora > 15)
                            rowObj.DefaultCellStyle.BackColor = Color.LightYellow;
                    }
                }

                // Configura formatos de columnas
                ventasDataGridView.Columns["KilosNeto"].DefaultCellStyle.Format = "N2";
                ventasDataGridView.Columns["Total"].DefaultCellStyle.Format = "C0";

                // Ajusta las columnas automáticamente
                ventasDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
            catch (Exception ex)
            {
                // Muestra un mensaje detallado del error
                MessageBox.Show($"Error al cargar las ventas del cliente: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

