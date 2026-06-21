# <img src="screenshots/lg.png" alt="wshare" width="28" style="vertical-align: middle;"> wshare

A lightweight local network file transfer tool built with **.NET** that enables browser-based file and folder transfers between devices on the same Wi-Fi network using a streaming HTTP server.

---

## 📡 Overview

`wshare` allows devices on the same local network to transfer files directly through a browser.

One device runs the server, and others connect using the displayed LAN address. Transfers are streamed over HTTP and written directly to disk in real time.

The server also exposes files stored inside the `downloads/` directory, allowing connected devices to browse and download files directly from the web interface.

---

## 💡 Inspiration

This project was built out of a real limitation.

I was brought a laptop where **USB access was restricted and software installation required admin privileges**, so traditional file transfer tools were not an option. To solve this, I built `wshare` to move files over a local network using only a browser.

---

> 💡 **Note:**
> Uploaded files are stored inside the `downloads/` directory.
> Files located there are automatically exposed through the download tab and can be downloaded by other devices connected to the same network.

---

## 📸 Screenshots

<table>
  <tr>
    <td width="50%">
      <img src="screenshots/ui.png" alt="wshare UI" style="width: 100%;" />
    </td>
    <td width="50%">
      <img src="screenshots/server.png" alt="wshare Server" style="width: 100%;" />
    </td>
  </tr>
</table>

---

## 📁 Project Structure

```text
wshare/
├── screenshots/
├── wshare.Server/
└── wshare-client/
```

---

## 💻 Running the Project

### 🚀 Primary (Development Mode)

```bash
cd wshare-client
npm install
npm run build

cd ../wshare.Server
dotnet run
```

The server prints LAN access URLs automatically.

Open one from another device connected to the same Wi-Fi network.

---

### 📦 Alternative (Published Build)

#### Linux

```bash
cd wshare.Server/bin/Release/net9.0/linux-x64/publish
chmod +x wshare.Server
./wshare.Server
```

#### Windows

```bash
wshare.Server.exe
```

---

## ⚙️ How it works

* Starts a .NET HTTP server on port `5050`
* Detects and prints active LAN IPs
* Serves React UI from `wwwroot`
* Accepts streamed multipart uploads
* Writes files directly to disk (no full buffering)
* Preserves folder structure during upload
* Exposes downloadable files from `downloads/`

---

## 🧱 Stack

* .NET
* React

---

## 📄 License

[MIT](./LICENSE)
