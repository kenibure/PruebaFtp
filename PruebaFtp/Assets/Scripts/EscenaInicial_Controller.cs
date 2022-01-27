using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscenaInicial_Controller : MonoBehaviour
{
    public Text textIpEsteDispo;
    public Text textIpServidor;
    public Text textHayConexion;
    public Text textProcesoCompleto;
    public Text logEnPantalla;

    private FtpWebRequest request;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            crearFicheroInicial(StaticUtilities.nombreArchivoDePrueba);
            anadirTextoALogEnPantalla("01");
            asignarIpEsteDispoAlFront();
            asignarIpServidorAlFront();
            anadirTextoALogEnPantalla("02");

            string nombreFicheroSinExtension = StaticUtilities.nombreArchivoDePrueba.Split('.')[0];
            string extensionFichero = "";
            if (StaticUtilities.nombreArchivoDePrueba.Split('.').Length > 0)
            {
                extensionFichero = StaticUtilities.nombreArchivoDePrueba.Split('.')[1];
            }

            string nombreFicheroGenerado = nombreFicheroSinExtension + "_" + StaticUtilities.obtenerFechaHoraActual() + "." + extensionFichero;
            anadirTextoALogEnPantalla("03");

            conectarAlServidorParaSubir(nombreFicheroGenerado);
            anadirTextoALogEnPantalla("04");
            subirFichero(request, StaticUtilities.rutaDeGuardadoEnEsteDispositivo, StaticUtilities.nombreArchivoDePrueba);
            anadirTextoALogEnPantalla("05");

            conectarAlServidorParaDescargar(nombreFicheroGenerado);
            anadirTextoALogEnPantalla("06");
            descargarFichero(request.GetResponse(), StaticUtilities.rutaDeGuardadoEnEsteDispositivo, nombreFicheroGenerado);
            anadirTextoALogEnPantalla("07");
            bool ficheroDescargado = comprobarSiExisteFichero(generarRuta(StaticUtilities.rutaDeGuardadoEnEsteDispositivo, nombreFicheroGenerado));
            
            anadirTextoALogEnPantalla("07");
            if(ficheroDescargado)
            {
                confirmarProcesoCompletado();
            }
        }
        catch (Exception ex)
        {
            anadirTextoALogEnPantalla(ex.ToString());
        }
    }

    private void asignarIpServidorAlFront()
    {
        textIpServidor.text = StaticUtilities.ipServidor;
    }

    private void asignarIpEsteDispoAlFront()
    {
        textIpEsteDispo.text = StaticUtilities.obtainLocalIp();
    }

    //Llamar a este metodo cuando se haya verificado que hay conexion con el servidor
    private void confirmarConexion()
    {
        textHayConexion.text = "SI";
    }
    private void confirmarProcesoCompletado()
    {
        textProcesoCompleto.text = "SI";
    }

    private void conectarAlServidorParaSubir(string nombreFicheroEnElFtp)
    {
        request = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + StaticUtilities.ipServidor + "/" + nombreFicheroEnElFtp));
        request.Method = WebRequestMethods.Ftp.UploadFile;
    }

    private void subirFichero(FtpWebRequest request, string rutaEnElDispositivo, string nombreDelArchivoEnElDispositivo)
    {
        //FileInfo fileInfo = new FileInfo(rutaEnElDispositivo + "/" + nombreDelArchivoEnElDispositivo);
        FileInfo fileInfo = new FileInfo(generarRuta(rutaEnElDispositivo, nombreDelArchivoEnElDispositivo));
        int buffLength = 2048;
        byte[] buff = new byte[buffLength];
        int contentLen;
        FileStream fs = fileInfo.OpenRead();
        Stream strm = request.GetRequestStream();
        confirmarConexion();


        contentLen = fs.Read(buff, 0, buffLength);
        while (contentLen != 0)
        {


            strm.Write(buff, 0, contentLen);
            contentLen = fs.Read(buff, 0, buffLength);
        }
        fs.Close();
        strm.Close();

    }

    private void conectarAlServidorParaDescargar(string nombreFichero)
    {
        request = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + StaticUtilities.ipServidor + "/" + nombreFichero));
        request.Method = WebRequestMethods.Ftp.DownloadFile;
    }

    void descargarFichero(WebResponse request, string rutaEnElDispositivo, string nombreDelArchivoEnElDispositivo)
    {
        Stream reader = request.GetResponseStream();
        confirmarConexion(); //Si llega hasta aqui significa que hay conexion.
        //if (!Directory.Exists(Path.GetDirectoryName(rutaEnElDispositivo)))
        if (!Directory.Exists(generarRuta(rutaEnElDispositivo, null)))
        {
            Directory.CreateDirectory(generarRuta(rutaEnElDispositivo, null));
        }

        FileStream fileStream = new FileStream(generarRuta(rutaEnElDispositivo, nombreDelArchivoEnElDispositivo), FileMode.Create);
        
        int bytesRead = 0;
        byte[] buffer = new byte[2048];

        while (true)
        {
            bytesRead = reader.Read(buffer, 0, buffer.Length);

            if (bytesRead == 0)
                break;

            fileStream.Write(buffer, 0, bytesRead);
        }
        fileStream.Close();
    }

    private bool comprobarSiExisteFichero(string path)
    {
        anadirTextoALogEnPantalla("path => |"+path+"|");
        return File.Exists(path);
    }

    private void anadirTextoALogEnPantalla(string nuevo)
    {
        string viejo = logEnPantalla.text;

        if (viejo == null)
        {
            viejo = "";
        }

        logEnPantalla.text = viejo + " // " + nuevo;
    }

    private void crearFicheroInicial(string field)
    {
        /*FileStream fileStream = new FileStream(generarRuta(directory, field), FileMode.Create);

        byte[] buffer = new byte[2048];

        fileStream.Write(buffer, 0, 99);

        fileStream.Close();*/

        //string path = generarRuta(directory, field);

#if UNITY_EDITOR
        string path = generarRuta(null, field);
#else
        // check if file exists in Application.persistentDataPath
        string path = string.Format("{0}/{1}", Application.persistentDataPath, field);
#endif

        anadirTextoALogEnPantalla("path donde se crea => |" + path + "|");

        FileStream fStream = File.Create(path);
        BinaryFormatter binary = new BinaryFormatter();
        binary.Serialize(fStream, "Texto con el que se crea en la APP un fichero de prueba.");
        fStream.Close();
    }

    private string generarRuta(string directory, string field)
    {
        //string path = Path.Combine(Application.persistentDataPath, directory);

        //string path = directory;

        //string path = Application.dataPath + "/" + directory;

        string path = null;
#if UNITY_EDITOR
        //path = string.Format(@"Assets/StreamingAssets/{0}", directory);
        path = "Assets/StreamingAssets";
        if (field != null)
        {
            path = path + "/" + field;
        }
#else
        // check if file exists in Application.persistentDataPath
        if (field == null)
        {
            anadirTextoALogEnPantalla("H01");
            path = Application.persistentDataPath;
        } else {
            anadirTextoALogEnPantalla("H02");
            path = string.Format("{0}/{1}", Application.persistentDataPath, field);
        }

#endif

        /*if (field != null)
        {
            path = path + "/" + field;
        }*/

        anadirTextoALogEnPantalla("path (02) => |" + path+"|");

        return path;
    }
}
