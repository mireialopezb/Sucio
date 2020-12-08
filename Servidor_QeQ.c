#include <string.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdio.h>
#include <mysql.h>
#include <ctype.h>
#include <pthread.h>
#include <string.h>
#define port 9091
#define MAX 100
//preferencias -std=c99 `mysql_config --cflags --libs`
//ejecucion gcc -o prop prog.c `mysql_config --cflags --libs`

int i;
int j;
int sockets[100];
//Estructura necesaria para acceso excluyente
pthread_mutex_t mutex;

typedef struct {
	char nombre [20];
	int Socket;
} Conectado;

typedef struct {
	Conectado conectados [MAX];
	int num;
} ListaConectados;

ListaConectados milista;

int Pon (ListaConectados *lista, char nombre [20], int Socket) 
	//funcion para anadir un cliente conectado a la lista
{
	if (lista->num == MAX)
		return -1; //devuelve -1 si la lista est\uffe1 llena
	else{
		
		strcpy(lista->conectados[lista->num].nombre,nombre);
		lista->conectados[lista->num].Socket=Socket;
		lista->num++;
		//printf("%s,%d",lista->conectados[lista->num].nombre,lista->conectados[lista->num].Socket);
		return 0;
		
	}
}

void Dameconectados (ListaConectados *lista, char conectados [512])
// pone en conectados todos los nombres separados por /, primero pone el numero de conectados
{
	strcpy(conectados,"");
	
	//indicamos cuantos usuarios estan conectados
	sprintf(conectados,"%d/", lista->num);
	
	for (int i=0;  i<lista->num; i++)
	{	
		// añadimos los usuarios conectados uno por uno
		sprintf(conectados, "%s%s,", conectados, lista->conectados[i].nombre);
	}
	
	if (lista->num==0)
		strcpy(conectados,"0");
}

int Damesocket (ListaConectados *lista, char nombre [20])
{ //Devuelve el socket o -1 si no esta en la lista
	int i = 0;
	int encontrado =0;
	while ((i<lista->num)&&(encontrado == 0))
	{
		if (strcmp(lista->conectados[i].nombre, nombre) == 0)
		{
			encontrado = 1;
			return lista->conectados[i].Socket;
		}
		i++;
	}
	if (!encontrado)
		return -1;
}



int Dameposicion (ListaConectados *lista, char nombre [20])
{ //Devuelve el socket o -1 si no esta en la lista
	int i = 0;
	int encontrado =0;
	while ((i<lista->num)&&(encontrado == 0))
	{
		if (strcmp(lista->conectados[i].nombre, nombre) == 0)
		{			
			//printf("%s, %s\n",nombre,lista->conectados[2].nombre);
			encontrado = 1;
			
			//printf("%d\n",i);
			return i;
		}
		i++;
	}
	if (!encontrado)
		return -1;
}


int Eliminar (ListaConectados *lista, char nombre[20])
	//Devuelve 0 si se ha eliminado correctamente, -1 si no esta en la lista
{
	int pos = Dameposicion (lista, nombre);
	printf("Posición %d nombre %s\n",pos,nombre);
	if (pos == -1)
		return -1;
	else
	{
		for (int i=pos; i < lista->num-1; i++)
		{//lista->conectados[i] = lista->conectados[i+1];
			strcpy(lista->conectados[i].nombre, lista->conectados[i+1].nombre);
			lista->conectados[i].Socket = lista->conectados[i+1].Socket;
		}
		lista->num --;
		return 0;
	}
}

void *AtenderCliente( void *socket)
{
	MYSQL *conn;	
	//creamos una conexión al servidor MYSQL
	conn=mysql_init(NULL);
	if(conn==NULL)
	{
		printf("Error al crear la conexión: %u %s\n",
			   mysql_errno(conn),mysql_error(conn));
		exit(1);
	}
	
	//inicializar la conexión
	//conn =mysql_real_connect(conn,"shiva2.upc.es","root","mysql","M4_juego",0,NULL,0);
	
	conn =mysql_real_connect(conn,"localhost","root","mysql","juego",0,NULL,0);
	
	if(conn==NULL)
	{
		printf("Error al iniciar la conexión: %u %s\n",
			   mysql_errno(conn), mysql_error(conn));
		exit(1);
	}
	
	int sock_conn, socket_invitador,socket_jugador2;
	int *s;
	s=(int *) socket;
	sock_conn=*s;
	//int sock_conn= * (int*) socket;
	
	
	int ret;
	char buff[512];
	char buff2[512];
	char notificacion [512];
	int err;
	MYSQL_ROW row;
	int rondas ,ID_ganador, duracion;
	int rondas_record=1000, ID_ganador_record=1000,duracion_record=100000;
	MYSQL_RES *resultado;
	char consulta [200];
	char nombre[20];
	
	int terminar=0;
	while (terminar ==0)
	{
		
		// Ahora recibimos el código, que dejamos en buff
		ret=read(sock_conn,buff, sizeof(buff));
		printf ("Recibido\n");
		
		// Tenemos que a?adirle la marca de fin de string 
		// para que no escriba lo que hay despues en el buffer
		buff[ret]='\0';
		
		//Escribimos el nombre en la consola
		
		printf ("Mensaje recibido: %s\n",buff);
		
		
		char *p = strtok( buff, "/");
		int codigo =  atoi (p);
		char contrasena[20],ID_jugador[10], ID_partida[10];
		
		if (codigo == 0)
			terminar=1;
		
		else if (codigo ==1) //iniciar sesion
		{
			p = strtok( NULL, "/");
			strcpy (nombre, p);
			p = strtok( NULL, "/");
			strcpy (contrasena, p);
			
			// construimos la consulta SQL
			err=mysql_query(conn,"SELECT * from jugador");
			if (err!=0)
			{
				printf("Error al consultar datos de la base %u %s\n",
					   mysql_errno(conn),mysql_error(conn));
				exit(1);
			}
			
			//recogemos el resultado de la consulta 
			resultado=mysql_store_result(conn);
			//Estructura matricial en memoria
			//cada fila contiene los datos de una partida
			
			//obtenemos los datos de una fila
			row=mysql_fetch_row(resultado);
			int encontrado=0;
			if (row==NULL)
				printf("No se han obtenido datos en la consulta\n");
			else
				while ((row !=NULL)&&(encontrado==0))
			{
					//recorre la base de datos para ver si el usuario existe
					if((strcmp(nombre,row[1])==0)&&(strcmp(contrasena,row[2])==0))
					{
						sprintf(buff2,"1/%s",row[0]);
						encontrado=1;
						pthread_mutex_lock(&mutex); //no me interrumpas ahora
						//Añadimos el usuario a la lista de conectados
						Pon(&milista,nombre,sock_conn);
						
						pthread_mutex_unlock(&mutex); //ya puedes interrumpirme
						
						//Envia la lista de conectados actualizada a todos los usuarios
						char conectados [512];
						strcpy(conectados, "");
						Dameconectados (&milista, conectados);
						strcpy(notificacion, "");
						sprintf(notificacion,"6/%s",conectados);
						
						
						for(int j =0; j<milista.num;j++)
						{
							write(milista.conectados[j].Socket,notificacion,strlen(notificacion));
							printf("%s\n",notificacion);
						}
						strcpy(notificacion,"");
					}
					row=mysql_fetch_row(resultado);
			}
				if (encontrado==0) //si no ha encontrado al usuario en la base de datos, envia un 0
					sprintf(buff2,"1/0");
		}
		
		else if (codigo==2) //registrarse
		{
			p = strtok( NULL, "/");
			strcpy (nombre, p);
			p = strtok( NULL, "/");
			strcpy (contrasena, p);	
			
			// construimos la consulta SQL
			err=mysql_query(conn,"SELECT * from jugador");
			if (err!=0)
			{
				printf("Error al consultar datos de la base %u %s\n",
					   mysql_errno(conn),mysql_error(conn));
				exit(1);
			}
			
			//recogemos el resultado de la consulta 
			resultado=mysql_store_result(conn);
			//Estructura matricial en memoria
			//cada fila contiene los datos de una partida
			
			//obtenemos los datos de una fila
			row=mysql_fetch_row(resultado);
			int encontrado=0;
			if (row==NULL)
				printf("No se han obtenido datos en la consulta\n");
			else
			{
				while ((row !=NULL)&&(encontrado==0))
				{
					//miramos si ya existe un jugador en la base de datos con el mismo nombre y contraseña
					if((strcmp(nombre,row[1])==0)&&(strcmp(contrasena,row[2])==0))
						//el jugador ya existe
						//envia un 0 al cliente para informar de que este jugador ya existe
					{
						strcpy(buff2,"2/0");
						encontrado=1;
					}
					row=mysql_fetch_row(resultado); //recorre toda la tabla
				}
				if (encontrado==0)//&&(strlen(nombre)!=0)&&(strlen(contrasena)!=0)) 
					//el jugador no existe, asi que lo añade a la base de datos
				{
					//como los ID van en orden (ej: 1,2,3,4...)
					//contamos cuantos jugadores hay registrados
					//el último id usado será igual al número de jugadores
					strcpy (consulta,"SELECT COUNT(*) FROM jugador");
					
					err=mysql_query (conn, consulta);
					if (err!=0) 
					{
						printf ("Error al consultar datos de la base %u %s\n",
								mysql_errno(conn), mysql_error(conn));
						exit (1);
					}
					
					
					resultado = mysql_store_result (conn);
					row = mysql_fetch_row (resultado);
					if (row == NULL)
						printf ("No se han obtenido datos en la consulta\n");
					else
						// la columna 0 contiene el ulitimo ID de jugador usado
						sprintf(ID_jugador,"%d",atoi(row[0])+1);
					
					//creamos la consulta
					pthread_mutex_lock(&mutex); //no me interrumpas ahora
					char consulta [80];
					strcpy (consulta, "INSERT INTO jugador VALUES (");
					//concatenamos el ID_jugador 
					strcat (consulta, ID_jugador); 
					strcat (consulta, ",'");
					//concatenamos el nombre 
					strcat (consulta, nombre); 
					strcat (consulta, "','");
					//concatenamos la contraseña
					strcat (consulta, contrasena); 
					strcat (consulta, "');");
					printf("%s",consulta);
					
					// Añadimos el usuario a la lista de conectados
					Pon(&milista,nombre,sock_conn);
					
						
					err = mysql_query(conn, consulta);
					if (err!=0) 
						printf ("Error al introducir datos la base %u %s\n", 
								mysql_errno(conn), mysql_error(conn));
					else
						//la inserción se ha realizado con exito
						//informamos al cliente enviando el ID_jugador asignado
						sprintf(buff2,"2/%s",ID_jugador);
					
					pthread_mutex_unlock(&mutex); //ya puedes interrumpirme
					
					char conectados [512];
					//Envia la lista de conectados actualizada a todos los usuarios
					Dameconectados (&milista, conectados);
					
					sprintf(notificacion,"6/%s",conectados);
					
					for(int j =0; j<i;j++)
					{
						write(milista.conectados[j].Socket,notificacion,strlen(notificacion));
						printf("%s\n",notificacion);
					}
					strcpy(notificacion,"");
				}

			}
		}
		
		else if (codigo==3) 
			//record
		{
			err=mysql_query(conn,"SELECT * from partida");
			if (err!=0)
			{
				printf("Error al consultar datos de la base %u %s\n",
					   mysql_errno(conn),mysql_error(conn));
				exit(1);
			}
			
			//recogemos el resultado de la consulta 
			resultado=mysql_store_result(conn);
			//Estructura matricial en memoria
			//cada fila contiene los datos de una partida
			
			//obtenemos los datos de una fila
			row=mysql_fetch_row(resultado);
			
			int i=0;
			if (row==NULL)
				printf("No se han obtenido datos en la consulta\n");
			else
				while (row !=NULL)
			{
					//guarda los valores de row en sus variables correspondientes
					//convirtiendolas a enteros
					duracion  =atoi(row[3]);
					ID_ganador = atoi (row[4]);
					rondas = atoi (row [5]);
					
					
					//compara los datos con los guardados en la variable
					//del jugador que mantiene el record
					if(rondas<rondas_record)
					{
						rondas_record = rondas;
						ID_ganador_record = ID_ganador;
						duracion_record = duracion;
					}
					else if((duracion<duracion_record)&&(rondas_record==rondas))
					{
						rondas_record = rondas;
						ID_ganador_record = ID_ganador;
						duracion_record = duracion;
					}
					
					row=mysql_fetch_row(resultado);
					
			}
				
				//convertimos ID_ganador_record en char para poder hacer la consulta
				char ID[20];
				sprintf(ID, "%d", ID_ganador_record);
				
				// construimos la consulta SQL
				strcpy (consulta,"SELECT nombre FROM jugador WHERE ID_jugador = '"); 
				strcat (consulta, ID);
				strcat (consulta,"'");
				
				//hacemos la consulta
				err=mysql_query(conn, consulta);
				if(err!=0)
				{
					printf ("Error al consultar datos de la base %u %s\n",
							mysql_errno(conn), mysql_error(conn));
					exit (1);
				}
				
				//recogemos el resultado de la consulta
				resultado=mysql_store_result(conn);
				row=mysql_fetch_row(resultado);
				if(row==NULL)
					printf("No se han obtenido datos de la consulta \n");
				else
					//matriz con una fila y una columna
					sprintf(buff2,"3/%s tiene el record con %d rondas.",row[0],rondas_record);
				printf("%s tiene el record con %d rondas\n",row[0],rondas_record);
		}
		
		else if (codigo==4)
			//ID de los personajes
		{
			p = strtok( NULL, "/");
			strcpy (ID_partida, p);
			
			sprintf(consulta,"SELECT personaje.nombre_personaje FROM (partida, personaje, registro) WHERE partida.ID_partida = %s AND partida.ID_partida = registro.ID_partida AND registro.ID_personaje = personaje.ID_personaje",ID_partida);
			
			err=mysql_query (conn, consulta);
			
			if (err!=0) {
				printf ("Error al consultar datos de la base %u %s\n",
						mysql_errno(conn), mysql_error(conn));
				exit (1);
			}
			//recogemos el resultado de la consulta
			resultado = mysql_store_result (conn);
			row = mysql_fetch_row (resultado);
			if (row == NULL)
			{
				printf ("No se han obtenido datos en la consulta\n");
				sprintf(buff2, "Esa partida no existe");
			}
			else 
			{
				sprintf(buff2,"4/Personaje 1: %s.", row[0]);
				row = mysql_fetch_row (resultado);
				char frase[100];
				sprintf (frase, " Personaje 2: %s", row[0]);
				strcat(buff2, frase);
			}
		}
		
		else if (codigo==5)
			//Cuantas partidas ha jugado un jugador
		{
			p = strtok( NULL, "/");
			strcpy (ID_jugador, p);
			
			
			char consulta [80];
			strcpy (consulta,"SELECT COUNT(*) FROM registro WHERE ID_jugador = '");
			strcat (consulta, ID_jugador);
			strcat (consulta,"'");
			
			
			err=mysql_query (conn, consulta);
			if (err!=0) {
				printf ("Error al consultar datos de la base %u %s\n",
						mysql_errno(conn), mysql_error(conn));
				exit (1);
			}
			
			resultado = mysql_store_result (conn);
			row = mysql_fetch_row (resultado);
			if (row == NULL)
				printf ("No se han obtenido datos en la consulta\n");
			else
				while (row !=NULL) 
			{
					// la columna 0 contiene el ID del jugador
					sprintf (buff2,"5/Ha jugado %s partidas", row[0]);
			}
				
		}
		
		else if (codigo == 7)
			// cuando el usuario invita a otra persona
			// el cleinte envia un mensaje al servidor con el formato:
			// 7/nombre de la persona a la que quiere invitar
		{
			char nombre_invitado [20];
			p = strtok( NULL, "/");
			strcpy (nombre_invitado, p);
			
			socket_jugador2 = Damesocket(&milista,nombre_invitado);
			
			printf("%d, %s\n",socket_jugador2,nombre);
			
			// creamos el mensaje que le llegara al invitado
			// que consiste en 7/nombre del usuario que invita
			sprintf (buff2,"7/%s",nombre);
			
			// enviamos el socket solo a la persona que queremos invitar
			write (socket_jugador2,buff2, strlen(buff2));
			printf("%s\n",buff2);
			strcpy(buff2,"");
			
		}
		
		else if (codigo == 8)
			// cuando invitan al cliente
			// el cliente envia un mensaje al servidor con el formato:
			// 8/nombre de la persona que le ha invitado/respuesta
		{
			char nombre_invitador [20];
			p = strtok( NULL, "/");
			strcpy (nombre_invitador, p);
			p = strtok( NULL, "/");
			char respuesta[1];
			strcpy (respuesta, p);
			
			socket_jugador2 = Damesocket(&milista,nombre_invitador);
			
			printf("%d, %s\n",socket_jugador2,nombre);
			
			// creamos el mensaje que le llegara a la persona que invita
			// que consiste en 8/respuesta
			sprintf (buff2,"8/%s",respuesta);
			
			//enviamos el socket solo a la persona que ha hecho la invitacion
			write (socket_jugador2,buff2, strlen(buff2));
			printf("%s\n",buff2);
			strcpy(buff2,"");
		}
		else if(codigo==9)
		{
			p = strtok( NULL, "/");
			char mensaje[400];
			strcpy (mensaje, p);
			sprintf(buff2,"9/%s",mensaje);
			write (socket_jugador2,buff2, strlen(buff2));
			printf("%d,%s\n",socket_jugador2,buff2);
		}
		
		if((codigo!=0) && (codigo!=7) && (codigo!= 8)&&(codigo!=9))
		{
			printf ("%s\n", buff2);
			// Y lo enviamos
			write (sock_conn,buff2, strlen(buff2));
			
			strcpy(buff2,"");
		}
		
			
		
	}
	
	pthread_mutex_lock(&mutex); //no me interrumpas ahora
	
	//eliminamos al usuario de la lista de conectados 
	int eliminar= Eliminar (&milista, nombre);
	
	pthread_mutex_unlock(&mutex); //ya puedes interrumpirme

	
	if (eliminar==0)
		printf("Se ha eliminado a %s de la lista de conectados",nombre);
	else
		printf("Error al eliminar a %s de la lista de conectados",nombre);
	
	//Envia la lista de conectados actualizada a todos los usuarios
	char conectados[512];
	Dameconectados (&milista, conectados);
	sprintf(notificacion,"6/%s",conectados);
	
	printf("%s\n",notificacion);
	
	//notificar a todos los clientes conectados
	for(int j =0; j<i;j++)
	{
		write(sockets[j],notificacion,strlen(notificacion));
	}
	strcpy(notificacion,"");
	//Desconectamos al usuario del servidor
	strcpy(buff2,"");
	close(sock_conn);
	mysql_close(conn);
}

int main(int argc, char *argv[])
{
	milista.num=0;
	
	int sock_conn, sock_listen, ret;
	struct sockaddr_in serv_adr;
	
	// INICIALITZACIONS
	// Obrim el socket
	if ((sock_listen = socket(AF_INET, SOCK_STREAM, 0)) < 0)
		printf("Error creant socket");
	// Fem el bind al port
	
	
	memset(&serv_adr, 0, sizeof(serv_adr));// inicialitza a zero serv_addr
	serv_adr.sin_family = AF_INET;
	
	// asocia el socket a cualquiera de las IP de la m?quina. 
	//htonl formatea el numero que recibe al formato necesario
	serv_adr.sin_addr.s_addr = htonl(INADDR_ANY);
	// escucharemos en el port 
	serv_adr.sin_port = htons(port);
	if (bind(sock_listen, (struct sockaddr *) &serv_adr, sizeof(serv_adr)) < 0)
		printf ("Error al bind");
	//La cola de peticiones pendientes no podr? ser superior a 4
	if (listen(sock_listen, 2) < 0)
		printf("Error en el Listen");
	
	
	// Atenderemos solo 7 peticione
	//for(int i=0;i<7;i++){
	pthread_t thread[100];
	i=0;
	j=0;
	
	for(;;)
	{ //bucle infinito
		printf ("Escuchando\n");
		
		sock_conn = accept(sock_listen, NULL, NULL);
		printf ("He recibido conexi?n\n");
		//sock_conn es el socket que usaremos para este cliente
		
		//Bucle de atención al cliente
		
		// Se acabo el servicio para este cliente
		sockets[i] =sock_conn;
		//sock_conn es el socket que usaremos para este cliente
		
		// Crear thead y decirle lo que tiene que hacer
		
		pthread_create (&thread[i], NULL, AtenderCliente,&sockets[i]);
		i++;
	}
	

	exit(0);
}

