using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace V1
{
    public partial class Form1 : Form
    {
        Socket server;
        Thread atender;
        string ID_jugador;
        int port = 9091;
        string ip = "192.168.1.55";
        //
        string directorio = "C:/Users/mirei/OneDrive/Documents/UNI/2B/SO/Proyecto_SO/V1/bin/Debug/Fotos/";
        int conectado = 0;
        string nombre, invitado,invitador;
        int num_conectados = 0;
        string[] conectados;
        List<string> lista_conectados = new List<string>();
        int seleccionado = 0;
        int personaje_seleccionado;

        public class CPersonaje
        {
            public string nombre, foto, foto_byn;
            public int byn; // cuando sea 0 la foto estara normal y cuando sea 1 en blanco y negro
        }

        public class CLista_Personajes
        {
            public int num = 15;
            public CPersonaje[] personaje;
        }

        public CLista_Personajes lista_personajes = new CLista_Personajes();

        public void rellenar_tabla_fotos(CLista_Personajes lista)
        {

            lista.personaje[1].nombre = "Emma";
            lista.personaje[2].nombre = "Antonia";
            lista.personaje[3].nombre = "Javi";
            lista.personaje[4].nombre = "Cristina_R";
            lista.personaje[5].nombre = "Julen";
            lista.personaje[6].nombre = "Arnau";
            lista.personaje[7].nombre = "Victor";
            lista.personaje[8].nombre = "Mireia";
            lista.personaje[9].nombre = "David";
            lista.personaje[10].nombre = "Gabri";
            lista.personaje[11].nombre = "Andrea";
            lista.personaje[12].nombre = "Enric";
            lista.personaje[13].nombre = "Izan";
            lista.personaje[14].nombre = "Angela";
            lista.personaje[15].nombre = "Cristina_B";

            for (int i = 1; i < 16; i++)
            {
                lista.personaje[i].foto = directorio + lista.personaje[i].nombre + ".jpg";
                lista.personaje[i].foto_byn = directorio + lista.personaje[i].nombre + "_byn.jpg";
            }

            panel_Emma.BackgroundImage = Image.FromFile(@lista.personaje[1].foto);
            panel_Antonia.BackgroundImage = Image.FromFile(@lista.personaje[2].foto);
            panel_Javi.BackgroundImage = Image.FromFile(@lista.personaje[3].foto);
            panel_Cristina_R.BackgroundImage = Image.FromFile(@lista.personaje[4].foto);
            panel_Julen.BackgroundImage = Image.FromFile(@lista.personaje[5].foto);
            panel_Arnau.BackgroundImage = Image.FromFile(@lista.personaje[6].foto);
            panel_Victor.BackgroundImage = Image.FromFile(@lista.personaje[7].foto);
            panel_Mireia.BackgroundImage = Image.FromFile(@lista.personaje[8].foto);
            panel_David.BackgroundImage = Image.FromFile(@lista.personaje[9].foto);
            panel_Gabri.BackgroundImage = Image.FromFile(@lista.personaje[10].foto);
            panel_Andrea.BackgroundImage = Image.FromFile(@lista.personaje[11].foto);
            panel_Enric.BackgroundImage = Image.FromFile(@lista.personaje[12].foto);
            panel_Izan.BackgroundImage = Image.FromFile(@lista.personaje[13].foto);
            panel_Angela.BackgroundImage = Image.FromFile(@lista.personaje[14].foto);
            panel_Cristina_B.BackgroundImage = Image.FromFile(@lista.personaje[15].foto);
        }

        delegate void DelegadoParaActualizar(string mensaje);

        delegate void DelegadoParaHacerVisible();

        public Form1()
        {
            InitializeComponent();
            //CheckForIllegalCrossThreadCalls = false;
            // Para que los elementos de los formularios puedan ser accedidos
            // desde threads diferentes
            
            lista_personajes.personaje = new CPersonaje[16];
            int i = 0;
            while (i < 16)
            {
                lista_personajes.personaje[i] = new CPersonaje();
                i = i + 1;
            }
            rellenar_tabla_fotos(lista_personajes);
        }

        private void Actualiza_Grid(string mensaje)
            // actualiza la lista de conectados cada vez que se conecte un usuario
        {
            Conectados_Grid.ColumnCount = 1;
            Conectados_Grid.RowCount = num_conectados;

            conectados = mensaje.Split(',');

            for (int i = 0; i < num_conectados; i++)
                Conectados_Grid.Rows[i].Cells[0].Value = conectados[i];
        }

        private void Actualiza_Invitacion_Label(string mensaje)
        {
            invitacion_label.Text = mensaje + " te ha invitado a una partida";
            Invitacion_groupBox.Visible = true;
        }

        private void Actualiza_V_Conectarse()
        {
            groupBox_inciar.Visible = false;
            groupBox_registro.Visible = false;
            groupBox_consultas.Visible = true ;
            Conectados_groupBox.Visible = true;
        }

        private void Chat(string mensaje)
        {
            Chat_listBox.Items.Add(mensaje);
        }

        private void AtenderServidor()
        {
            while (true)
            {
                // Recibimos la respuesta del servidor
                byte[] msg2 = new byte[80];
                server.Receive(msg2);
                string[] trozos = Encoding.ASCII.GetString(msg2).Split('/');

                int codigo = Convert.ToInt32(trozos[0]);
                string mensaje;
                if (codigo == 6)
                    // el formato del mensaje sera 6/numero de conectados/lista
                {
                    num_conectados = Convert.ToInt32(trozos[1]);
                    mensaje = trozos[2].Split('\0')[0];
                }
                else
                    mensaje = trozos[1].Split('\0')[0];

                switch (codigo)
                {
                    case 1: // Iniciar sesion
                        //La respuesta sera 0 si no se ha encontrado el usuario en labase de datos, sinó enviara su ID
                        if (mensaje == "0")
                            MessageBox.Show("Este usuario no existe");
                        else
                        {
                            ID_jugador = mensaje;
                            Invoke(new DelegadoParaHacerVisible(Actualiza_V_Conectarse));
                            MessageBox.Show("Se ha iniciado sesión correctamente, tu ID de jugador es: " + mensaje);
                        }
                        break;

                    case 2: // Registrarse
                        //La respuesta será 0 si se ha encontrado el usuario en labase de datos, sinó enviara su ID
                        if (mensaje == "0")
                            MessageBox.Show("Este usuario ya existe");
                        else
                        {
                            ID_jugador = mensaje;
                            Invoke(new DelegadoParaHacerVisible(Actualiza_V_Conectarse));
                            MessageBox.Show("Te has registrado correctamente, tu ID de jugador es: " + mensaje);

                        }

                        break;

                    case 3: // quien tiene el record
                        MessageBox.Show(mensaje);
                        break;

                    case 4: // que personajes se escogieron en la partida
                        MessageBox.Show(mensaje);
                        break;

                    case 5: // cuantas partidas ha jugado el jugador
                        MessageBox.Show(mensaje);
                        break;

                    case 6: // conectados
                        Conectados_Grid.Invoke(new DelegadoParaActualizar(Actualiza_Grid), new object[] { mensaje });
                        break;

                    case 7: // ivitación
                        invitador = mensaje;
                        invitacion_label.Invoke(new DelegadoParaActualizar(Actualiza_Invitacion_Label), new object [] { mensaje });
                        break;

                    case 8: // respuesta a la invitacion
                        // si han aceptado la invitación la respuesta será 1, sinó será 0
                        int respuesta = Convert.ToInt32(mensaje);
                        if (respuesta == 0)
                            MessageBox.Show("Han rechazado tu invitación");
                        else
                        {
                            MessageBox.Show("Han aceptado tu invitación");

                            /*Conectados_groupBox.Visible = false;
                            Tablero.Visible = true;
                            Turno.Visible = true;
                            Pregunta.Visible = true;
                            Responde.Visible = true;
                            Chat_groupBox.Visible = true;*/
                        }
                        //groupBox1.Visible = false;
                        break;
                    case 9: //recibe un mensaje en el chat
                        Chat_listBox.Invoke(new DelegadoParaActualizar(Chat), new object[] { mensaje });
                        break;


                }
            }
        }

        private void Consulta_Button_Click(object sender, EventArgs e)
        {
            if (conectado == 0)
            {
                //Creamos un IPEndPoint con el ip del servidor y puerto del servidor 
                //al que deseamos conectarnos
                IPAddress direc = IPAddress.Parse(ip);
                IPEndPoint ipep = new IPEndPoint(direc, port);


                //Creamos el socket 
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    server.Connect(ipep);//Intentamos conectar el socket               
                    //MessageBox.Show("Conectado");
                    conectado = 1;
                }
                catch (SocketException)
                {
                    //Si hay excepcion imprimimos error y salimos del programa con return 
                    MessageBox.Show("No he podido conectar con el servidor");
                    return;
                }

            }

            if (Record.Checked)
            {
                // Quiere saber quien tiene el record
                string mensaje = "3/";
                // Enviamos al servidor el codigo
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);
            }

            else if (Personajes.Checked)
            {
                // Quiere saber que personajes se escogieron en la partida seleccionada
                string mensaje = "4/" + ID_Partida.Text;
                // Enviamos al servidor el ID de la partida tecleado
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);
                ID_Partida.Text = "";
            }

            else if (Partidas.Checked)
            {
                //Quiere cuantas partidas ha jugado el jugador seleccionado
                string mensaje = "5/" + ID_Jugador.Text;
                // Enviamos al servidor el nombre y la altura tecleados
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);
            }
        }

        private void Registrarse_Button_Click(object sender, EventArgs e)
        {
            if (conectado == 0)
            {
                //Creamos un IPEndPoint con el ip del servidor y puerto del servidor 
                //al que deseamos conectarnos
                IPAddress direc = IPAddress.Parse(ip);
                IPEndPoint ipep = new IPEndPoint(direc, port);


                //Creamos el socket 
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    server.Connect(ipep);//Intentamos conectar el socket               
                    //MessageBox.Show("Conectado");
                    conectado = 1;
                }
                catch (SocketException)
                {
                    //Si hay excepcion imprimimos error y salimos del programa con return 
                    MessageBox.Show("No he podido conectar con el servidor");
                    return;
                }

                // pongo en marcha el thread que atendera los mensajes del servidor
                ThreadStart ts = delegate { AtenderServidor(); };
                atender = new Thread(ts);
                atender.Start();

               

            }
            if ((Nombre_Registro.Text == "") || (Contraseña_Registro.Text == ""))
                MessageBox.Show("No se han rellenado correctamente todos los campos");
            else
            {
                nombre = Nombre_Registro.Text;
                string msj = "2/" + Nombre_Registro.Text + "/" + Contraseña_Registro.Text;
                // Enviamos al servidor el nombre y la contraseña del tecleado
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(msj);
                server.Send(msg);
            }
        }

        private void Iniciar_Button_Click(object sender, EventArgs e)
        {
            if (conectado == 0)
            {
                //Creamos un IPEndPoint con el ip del servidor y puerto del servidor 
                //al que deseamos conectarnos
                IPAddress direc = IPAddress.Parse(ip);
                IPEndPoint ipep = new IPEndPoint(direc, port);


                //Creamos el socket 
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    server.Connect(ipep);//Intentamos conectar el socket               
                    //MessageBox.Show("Conectado");
                    conectado = 1;
                }
                catch (SocketException)
                {
                    //Si hay excepcion imprimimos error y salimos del programa con return 
                    MessageBox.Show("No he podido conectar con el servidor");
                    return;
                }

                // pongo en marcha el thread que atendera los mensajes del servidor
                ThreadStart ts = delegate { AtenderServidor(); };
                atender = new Thread(ts);
                atender.Start();
            }

            nombre = Nombre.Text;

            string msj = "1/" + Nombre.Text + "/" + Contraseña.Text;
            // Enviamos al servidor el nombre y la contraseño tecleado
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(msj);
            server.Send(msg);

        }

        private void Desconectar_Click(object sender, EventArgs e)
        {

            string msj = "0/";
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(msj);
            server.Send(msg);

            // Nos desconectamos
            atender.Abort();
            server.Shutdown(SocketShutdown.Both);
            conectado = 0;
            server.Close();
            MessageBox.Show("Desconectado");
            groupBox_consultas.Visible = false;
            groupBox_inciar.Visible = true;
            groupBox_registro.Visible = true;
        }

        private void Invitar_Click(object sender, EventArgs e)
        {
            string msj = "7/" + invitado;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(msj);
            server.Send(msg);
            groupBox1.Visible = true;
        }

        private void aceptar_button_Click(object sender, EventArgs e)
        //Cuando el cliente acepta la invitacion enviamos un 1 al servidor
        {
            string msj = "8/" + invitador + "/1";
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(msj);
            server.Send(msg);

            Invitacion_groupBox.Visible = false;
            /*Conectados_groupBox.Visible = false;
            Tablero.Visible = true;
            Turno.Visible = true;
            Pregunta.Visible = true;
            Responde.Visible = true;
            Chat_groupBox.Visible = true;*/

        }

        private void rechazar_button_Click(object sender, EventArgs e)
        //Cuando el cliente rechaza la invitacion enviamos un 0 al servidor
        {
            string msj = "8/" + nombre + "/0";
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(msj);
            server.Send(msg);

            Invitacion_groupBox.Visible = false;
        }

        private void Conectados_Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int fila = e.RowIndex;
            invitado = conectados[fila];
        }

        private void Emma_button_Click(object sender, EventArgs e)
        {
            if (seleccionado == 0)
            {
                personaje_seleccionado = 1;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[1].byn == 0)
                {
                    panel_Emma.BackgroundImage = Image.FromFile(@lista_personajes.personaje[1].foto_byn);
                    lista_personajes.personaje[1].byn = 1;
                }
                else
                {
                    panel_Emma.BackgroundImage = Image.FromFile(@lista_personajes.personaje[1].foto);
                    lista_personajes.personaje[1].byn = 0;
                }
            }
        }
        
        private void Seleciconar_button_Click(object sender, EventArgs e)
        {
            seleccionado = 1;
        }

        private void Antonia_button_Click(object sender, EventArgs e)
        {
            if (seleccionado == 0)
            {
                personaje_seleccionado = 2;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[2].byn == 0)
                {
                    panel_Antonia.BackgroundImage = Image.FromFile(@lista_personajes.personaje[2].foto_byn);
                    lista_personajes.personaje[2].byn = 1;
                }
                else
                {
                    panel_Antonia.BackgroundImage = Image.FromFile(@lista_personajes.personaje[2].foto);
                    lista_personajes.personaje[2].byn = 0;
                }
            }
        }

        private void Javi_button_Click(object sender, EventArgs e)
        {
            if (seleccionado == 0)
            {
                personaje_seleccionado = 3;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[3].byn == 0)
                {
                    panel_Javi.BackgroundImage = Image.FromFile(@lista_personajes.personaje[3].foto_byn);
                    lista_personajes.personaje[3].byn = 1;
                }
                else
                {
                    panel_Javi.BackgroundImage = Image.FromFile(@lista_personajes.personaje[3].foto);
                    lista_personajes.personaje[3].byn = 0;
                }
            }
        }

        private void CristinaR_button_Click(object sender, EventArgs e)
        {
            if (seleccionado == 0)
            {
                personaje_seleccionado = 4;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[4].byn == 0)
                {
                    panel_Cristina_R.BackgroundImage = Image.FromFile(@lista_personajes.personaje[4].foto_byn);
                    lista_personajes.personaje[4].byn = 1;
                }
                else
                {
                    panel_Cristina_R.BackgroundImage = Image.FromFile(@lista_personajes.personaje[4].foto);
                    lista_personajes.personaje[4].byn = 0;
                }
            }

        }

        private void Julen_button_Click(object sender, EventArgs e)
        {
            if (seleccionado == 0)
            {
                personaje_seleccionado = 5;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[5].byn == 0)
                {
                    panel_Julen.BackgroundImage = Image.FromFile(@lista_personajes.personaje[5].foto_byn);
                    lista_personajes.personaje[5].byn = 1;
                }
                else
                {
                    panel_Julen.BackgroundImage = Image.FromFile(@lista_personajes.personaje[5].foto);
                    lista_personajes.personaje[5].byn = 0;
                }
            }
        }

        private void Arnau_button_Click(object sender, EventArgs e)
        {
            if (seleccionado == 0)
            {
                personaje_seleccionado = 6;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[6].byn == 0)
                {
                    panel_Arnau.BackgroundImage = Image.FromFile(@lista_personajes.personaje[6].foto_byn);
                    lista_personajes.personaje[6].byn = 1;
                }
                else
                {
                    panel_Arnau.BackgroundImage = Image.FromFile(@lista_personajes.personaje[6].foto);
                    lista_personajes.personaje[6].byn = 0;
                }
            }
        }

        private void Victor_button_Click(object sender, EventArgs e)
        {
            if (seleccionado == 0)
            {
                personaje_seleccionado = 7;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[7].byn == 0)
                {
                    panel_Victor.BackgroundImage = Image.FromFile(@lista_personajes.personaje[7].foto_byn);
                    lista_personajes.personaje[7].byn = 1;
                }
                else
                {
                    panel_Victor.BackgroundImage = Image.FromFile(@lista_personajes.personaje[7].foto);
                    lista_personajes.personaje[7].byn = 0;
                }
            }
        }

        private void Mireia_button_Click(object sender, EventArgs e)
        {
            int numero = 8;

            if (seleccionado == 0)
            {
                personaje_seleccionado = numero;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[numero].byn == 0)
                {
                    panel_Mireia.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto_byn);
                    lista_personajes.personaje[numero].byn = 1;
                }
                else
                {
                    panel_Mireia.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto);
                    lista_personajes.personaje[numero].byn = 0;
                }
            }
        }

        private void David_Button_Click(object sender, EventArgs e)
        {
            int numero = 9;

            if (seleccionado == 0)
            {
                personaje_seleccionado = numero;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[numero].byn == 0)
                {
                    panel_David.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto_byn);
                    lista_personajes.personaje[numero].byn = 1;
                }
                else
                {
                    panel_David.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto);
                    lista_personajes.personaje[numero].byn = 0;
                }
            }
        }

        private void Gabri_button_Click(object sender, EventArgs e)
        {
            int numero = 10;

            if (seleccionado == 0)
            {
                personaje_seleccionado = numero;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[numero].byn == 0)
                {
                    panel_Gabri.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto_byn);
                    lista_personajes.personaje[numero].byn = 1;
                }
                else
                {
                    panel_Gabri.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto);
                    lista_personajes.personaje[numero].byn = 0;
                }
            }
        }

        private void Andrea_button_Click(object sender, EventArgs e)
        {
            int numero = 11;

            if (seleccionado == 0)
            {
                personaje_seleccionado = numero;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[numero].byn == 0)
                {
                    panel_Andrea.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto_byn);
                    lista_personajes.personaje[numero].byn = 1;
                }
                else
                {
                    panel_Andrea.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto);
                    lista_personajes.personaje[numero].byn = 0;
                }
            }
        }

        private void Enric_button_Click(object sender, EventArgs e)
        {
            int numero = 12;

            if (seleccionado == 0)
            {
                personaje_seleccionado = numero;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[numero].byn == 0)
                {
                    panel_Enric.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto_byn);
                    lista_personajes.personaje[numero].byn = 1;
                }
                else
                {
                    panel_Enric.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto);
                    lista_personajes.personaje[numero].byn = 0;
                }
            }
        }

        private void Izan_button_Click(object sender, EventArgs e)
        {
            int numero = 13;

            if (seleccionado == 0)
            {
                personaje_seleccionado = numero;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[numero].byn == 0)
                {
                    panel_Izan.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto_byn);
                    lista_personajes.personaje[numero].byn = 1;
                }
                else
                {
                    panel_Izan.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto);
                    lista_personajes.personaje[numero].byn = 0;
                }
            }
        }

        private void Angela_button_Click(object sender, EventArgs e)
        {
            int numero = 14;

            if (seleccionado == 0)
            {
                personaje_seleccionado = numero;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[numero].byn == 0)
                {
                    panel_Angela.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto_byn);
                    lista_personajes.personaje[numero].byn = 1;
                }
                else
                {
                    panel_Angela.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto);
                    lista_personajes.personaje[numero].byn = 0;
                }
            }
        }

        private void CristinaB_button_Click(object sender, EventArgs e)
        {
            int numero = 15;

            if (seleccionado == 0)
            {
                personaje_seleccionado = numero;
                panel_Seleccionado.BackgroundImage = Image.FromFile(@lista_personajes.personaje[personaje_seleccionado].foto);
            }

            else
            {
                if (lista_personajes.personaje[numero].byn == 0)
                {
                    panel_Cristina_B.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto_byn);
                    lista_personajes.personaje[numero].byn = 1;
                }
                else
                {
                    panel_Cristina_B.BackgroundImage = Image.FromFile(@lista_personajes.personaje[numero].foto);
                    lista_personajes.personaje[numero].byn = 0;
                }
            }
        }

        private void Enviar_Click(object sender, EventArgs e)
        {
            string msj = "9/" + nombre + ": " + Chat_TextBox.Text;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(msj);
            server.Send(msg);

            string texto_chat = "Tú: " + Chat_TextBox.Text;
            Chat_listBox.Items.Add(texto_chat);

            Chat_TextBox.Text = "";
        }

        



    }
}

