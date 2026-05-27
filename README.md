# <img src="screenshots/lg.png" alt="wshare" width="28" style="vertical-align: middle;"> wshare

A lightweight local network file transfer tool built with **.NET** that enables browser-based file and folder transfers between devices on the same Wi-Fi network using a streaming HTTP server.

---

## 📡 Overview

`wshare` allows devices on the same local network to transfer files directly through a browser.

One device runs the server, and others connect using the displayed LAN address. Transfers are streamed over HTTP and written directly to disk in real time.

---

## 💡 Inspiration

This project was built out of a real limitation

I was brought a laptop where **USB access was restricted and software installation required admin privileges**, so I couldn’t use traditional file transfer tools. To solve this, I built `wshare` to move files over a local network using only a browser.

---


> 💡 **Note:**  
> This project currently supports **one-way transfer only (client → server)**.  
> There is no download UI yet for sending files back from the server device to other devices.

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

This is the recommended way to run the project:

```bash
cd wshare-client
npm install
npm run build

cd ../wshare.Server
dotnet run
```

The server will print LAN access URLs automatically.  
Open one from another device on the same Wi-Fi network.

---

### 📦 Alternative (Published Build)

After publishing the server:

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

- Starts a .NET HTTP server on port `5050`
- Detects and prints active LAN IPs
- Serves React UI from `wwwroot`
- Accepts streamed multipart uploads
- Writes files directly to disk (no full buffering)


---

## 🧱 Stack

- .NET
- React

---

## 📄 License

[MIT](./LICENSE)