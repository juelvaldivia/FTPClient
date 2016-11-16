using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ftp
{
    public class DASH
    {
        public static bool existeAlFinal(string texto)
        {
            var dash = texto.Substring(texto.Length - 1, 1);

            if (dash == "/")
            {
                return true;
            }
            return false;
        }
    }
    public class Archivo
    {
        public static string obtenerNombre(string nombreArchivo)
        {
            return Path.GetFileName(nombreArchivo);
        }
    }
    public class FTP
    {
        /// <summary>
        /// Declaración de propiedades de FTP
        /// </summary>
        private string Servidor { get; set; }
        private string Usuario { get; set; }
        private string Passsword { get; set; }
        
        
        /// <summary>
        /// Constructor de la clase FTP
        /// </summary>
        /// <param name="servidor"></param>
        /// <param name="usuario"></param>
        /// <param name="password"></param>
        public FTP(string servidor, string usuario, string password)
        {
            this.Servidor = servidor;
            this.Usuario = usuario;
            this.Passsword = password;
        }

        /// <summary>
        /// obtener la ruta destino FTP para descargar o subir archivos
        /// </summary>
        /// <param name="carpeta"></param>
        /// <param name="archivo"></param>
        /// <returns></returns>
        private string getRuta(string carpeta, string archivo)
        {
            var ruta = Servidor;
            
            if (!DASH.existeAlFinal(Servidor))
            {
                ruta += "/";
            }

            if (carpeta != "")
            {
                ruta += carpeta;

                if (!DASH.existeAlFinal(carpeta))
                {
                    ruta += carpeta + "/";
                }
            }

            ruta += archivo;

            return ruta;
        }

        /// <summary>
        /// Descarga un archivo de un servicor FTP
        /// </summary>
        /// <param name="carpetaOrigen"></param>
        /// <param name="carpetaRemota"></param>
        /// <param name="archivoRemoto"></param>
        /// <returns>True o False</returns>
        public bool descargarArchivo(string carpetaOrigen, string carpetaRemota, string archivoRemoto)
        {
            string directorioRemoto = getRuta(carpetaRemota, archivoRemoto);

            try
            {
                FtpWebRequest peticion = ((FtpWebRequest)FtpWebRequest.Create(directorioRemoto));

                // Los datos del usuario (credenciales)
                NetworkCredential credenciales = new NetworkCredential(Usuario, Passsword);
                peticion.Credentials = credenciales;

                // El comando a ejecutar usando la enumeración de WebRequestMethods.Ftp
                peticion.Method = WebRequestMethods.Ftp.DownloadFile;

                // Obtener el resultado del comando
                StreamReader lector = new StreamReader(peticion.GetResponse().GetResponseStream());

                // Leer el stream
                string contenidoArchivoRemoto = lector.ReadToEnd();
                
                // Guardarlo localmente con la extensión
                string ficheroLocal = Path.Combine(carpetaOrigen, Path.GetFileName(directorioRemoto));
                StreamWriter escritor = new StreamWriter(ficheroLocal, false, Encoding.UTF8);

                escritor.Write(contenidoArchivoRemoto);
                escritor.Close();

                // Cerrar el stream abierto.
                lector.Close();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        /// <summary>
        /// Descarga multiples archivos de un servidor
        /// </summary>
        /// <param name="carpetaOrigen"></param>
        /// <param name="carpetaRemota"></param>
        /// <param name="destinos"></param>
        public void descargarArchivos(string carpetaOrigen,string carpetaRemota, List<string> destinos)
        {
            foreach (string destino in destinos)
            {
                string nombreArchivo = Archivo.obtenerNombre(destino);

                if (descargarArchivo(carpetaOrigen, carpetaRemota, destino))
                {
                    Console.WriteLine("se descargó el archivo: " + nombreArchivo);
                }
                else
                {
                    Console.WriteLine("no se descargó el archivo: " + nombreArchivo);
                }
            }
        }
        /// <summary>
        /// Sube archivo a un servidor FTP
        /// </summary>
        /// <param name="rutaOrigen"></param>
        /// <param name="rutaDestino"></param>
        /// <returns>True o False</returns>
        public bool subirArchivo(string rutaOrigen, string rutaDestino)
        {
            string nombreArchivo = Archivo.obtenerNombre(rutaOrigen);
            
            string archivoRemoto = getRuta(rutaDestino, nombreArchivo);

            try
            {
                // objeto de peticion por ftp
                FtpWebRequest peticion = (FtpWebRequest)FtpWebRequest.Create(archivoRemoto);
                peticion.Proxy = null;
                peticion.UsePassive = true;
                peticion.UseBinary = true;
                peticion.KeepAlive = true;
                // metodo
                peticion.Method = WebRequestMethods.Ftp.UploadFile;

                // datos del usuario
                peticion.Credentials = new NetworkCredential(Usuario, Passsword);

                //Copiamos el contenido del archivo del equipo al que creamos en el server
                StreamReader lector = new StreamReader(rutaOrigen);
                byte[] contenidoArchivo = Encoding.UTF8.GetBytes(lector.ReadToEnd());
                lector.Close();
                peticion.ContentLength = contenidoArchivo.Length;

                //Generamos la peticion de Stream para poder transferir los datos
                Stream escritor = peticion.GetRequestStream();
                escritor.Write(contenidoArchivo, 0, contenidoArchivo.Length);
                escritor.Close();

                FtpWebResponse respuesta = (FtpWebResponse)peticion.GetResponse();

                //Cerramos el hilo por el cual se subió el archivo
                respuesta.Close();

                return true;


            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Sube multiples archivos a un servidor FTP
        /// </summary>
        /// <param name="origenes"></param>
        /// <param name="destino"></param>
        public void subirArchivos(List<string> origenes, string destino){

            foreach (string origen in origenes)
            {
                string nombreArchivo = Archivo.obtenerNombre(origen);

                if (subirArchivo(origen, destino))
                {
                    Console.WriteLine("se subió el archivo: " + nombreArchivo);
                }
                else
                {
                    Console.WriteLine("no se subió el archivo: " + nombreArchivo);
                }
            }
        }

    }


    class Program
    {
        static void Main(string[] args)
        {
            var servidor = "";
            var usuario = "";
            var contra = "";

            FTP ftp = new FTP(servidor, usuario ,contra );

            var rutaOrigen = @"c:\ejemploFTP.txt";
            var rutaDestino = "";

            if (ftp.subirArchivo(rutaOrigen, rutaDestino))
            {
                Console.WriteLine("Se subió");

            }
            else
            {
                Console.WriteLine("No se descargo hubo un error");
            }

            Console.ReadLine();
        }

    }
}


      
    
