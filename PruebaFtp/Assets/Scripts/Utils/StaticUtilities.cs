using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class StaticUtilities
{
    public static string ipServidor = "192.168.1.130"; //Es la IP local donde esta el servidor FTP.
    public static string nombreArchivoDePrueba = "file03.txt";

    //Devuelve la IP local del dispositivo.
    public static string obtainLocalIp()
    {
        IPHostEntry host;
        string localIP = "0.0.0.0";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }
    public static string obtenerFechaHoraActual()
    {
        return System.DateTime.UtcNow.AddHours(1).ToString("yyyy-MM-dd_HH-mm-ss");
    }
}
