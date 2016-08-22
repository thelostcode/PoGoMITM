# PoGo-Proxy.NET

This project is a .net MITM proxy designed to read all the API messages sent between the Pokemon Go device and the Pokemon Go servers. By reading this data, you can make informed decisions about which Pokemon to keep, and so on.

The [POGOProtocs](https://github.com/AeonLucid/POGOProtos) repo is used as a source for protoc files.

# Usage

## Setting Up

In order to get this up a running, you have to run through a couple of steps:

* First modify the Ip settings in the ProxyController.cs file
  * Find your local ip address and set that as the string Ip at the top of the ProxyController class (By default, it's binding all local ips)
  * Keep the port as is or update the port to whatever you want

* Then, run the project once to generate the CA certificate. This will add the certificate to your computer. You also need to export this certificate and add it to your android device.

* Set your Pokemon Go device to use the proxy config

* Go to Settings/Wifi, find your Wifi connection and modify it to use the proxy ip / port

* Go to http://mitm.it/install-cert on your mobile and click to "Download Certificate". If it fails to install, please download the certificate and manually add it to your WIFI settings.


## Getting started

Once you have everything set up, just start up the sample project on your host device, then open the Pokemon Go app on your phone/tablet. You should start to see the api requests and responses populate the console window. More detailed logs are created in the logs folder.

## Certificate Pinning

As of version 0.31, Niantic added certificate pinning to Pokemon Go. In order to use this MITM proxy, you have to do a little extra work. You can either modify the apk to accept other certificates [as shown here](https://eaton-works.com/2016/07/31/reverse-engineering-and-removing-pokemon-gos-certificate-pinning/), or you can use something like XPosed to get around the certificate pinning on the fly.

In case of any problems, please follow instructions on https://github.com/rastapasta/pokemon-go-mitm

# Dependencies

This project is built in Visual Studio 2015 with the following nuget packages:
* DotNetZip v1.9.8
* Google.Protobuf v3.0.0
* Newtonsoft.Json v9.0.1
* Titanium.Web.Proxy v2.3000.185 TBulbaDB Fork (https://github.com/TBulbaDB/Titanium-Web-Proxy)

