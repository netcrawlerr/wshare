import { useState, useRef, useEffect, type ChangeEvent } from "react";

interface StatusState {
  type: "success" | "error" | "info" | "";
  text: string;
}

interface ServerFile {
  name: string;
  size: string;
  path: string;
}

export default function App() {
  const [activeTab, setActiveTab] = useState<"upload" | "download">("upload");
  const [uploading, setUploading] = useState<boolean>(false);
  const [progress, setProgress] = useState<number>(0);
  const [status, setStatus] = useState<StatusState>({ type: "", text: "" });
  const [serverFiles, setServerFiles] = useState<ServerFile[]>([]);

  const fileInputRef = useRef<HTMLInputElement>(null);
  const folderInputRef = useRef<HTMLInputElement>(null);

  const currentHost = window.location.hostname;
  const baseUrl = `http://${currentHost}:5050`;

  const fetchServerFiles = async () => {
    try {
      const response = await fetch(`${baseUrl}/api/files`);
      if (response.ok) {
        const data = await response.json();
        setServerFiles(data);
      } else {
        setStatus({
          type: "error",
          text: "SYS_ERR: FAILED_TO_FETCH_REGISTRY.",
        });
      }
    } catch (err) {
      setStatus({ type: "error", text: "SYS_ERR: REGISTRY_LINK_TIMEOUT." });
    }
  };

  useEffect(() => {
    if (activeTab === "download") {
      fetchServerFiles();
    }
  }, [activeTab]);

  const handleUpload = async (fileList: FileList | null) => {
    if (!fileList || fileList.length === 0) return;

    setUploading(true);
    setProgress(0);
    setStatus({ type: "info", text: "SYS_MSG: INITIALIZING_DATA_STREAM..." });

    const formData = new FormData();
    for (let i = 0; i < fileList.length; i++) {
      const file = fileList[i];
      const relativePath = (file as any).webkitRelativePath || file.name;
      formData.append("files", file, relativePath);
    }

    const xhr = new XMLHttpRequest();
    xhr.open("POST", `${baseUrl}/api/upload`, true);

    xhr.upload.onprogress = (event: ProgressEvent) => {
      if (event.lengthComputable) {
        const percentComplete = Math.round((event.loaded / event.total) * 100);
        setProgress(percentComplete);
        setStatus({
          type: "info",
          text: `SYS_MSG: BLOCKS_STREAMING_AT_${percentComplete}%`,
        });
      }
    };

    xhr.onload = () => {
      if (xhr.status === 200) {
        setStatus({
          type: "success",
          text: "SYS_MSG: OPERATION_SUCCESSFUL. DATA_STORED.",
        });
      } else {
        setStatus({
          type: "error",
          text: `SYS_ERR: INTERRUPT_CODE_${xhr.status}`,
        });
      }
      setUploading(false);
    };

    xhr.onerror = () => {
      setStatus({ type: "error", text: "SYS_ERR: LINK_LAYER_DISCONNECTED." });
      setUploading(false);
    };

    xhr.send(formData);
  };

  const handleDownloadFile = (file: ServerFile) => {
    setStatus({
      type: "info",
      text: `SYS_MSG: DOWNLOADING // TARGET: ${file.name}`,
    });

    window.open(
      `${baseUrl}/api/download?path=${encodeURIComponent(file.path)}`,
      "_blank",
    );
  };

  const onFileChange = (e: ChangeEvent<HTMLInputElement>) =>
    handleUpload(e.target.files);

  const renderTerminalProgress = () => {
    const totalBlocks = 20;
    const filledBlocks = Math.round((progress / 100) * totalBlocks);
    const emptyBlocks = totalBlocks - filledBlocks;
    return `[${"█".repeat(filledBlocks)}${"░".repeat(emptyBlocks)}] ${progress}%`;
  };

  return (
    <div
      className="relative flex min-h-screen items-center justify-center bg-[#090d16] p-4 font-mono text-emerald-500 selection:bg-emerald-500 selection:text-black overflow-hidden"
      style={{
        backgroundImage: `
          linear-gradient(rgba(16, 185, 129, 0.015) 1px, transparent 1px),
          linear-gradient(90deg, rgba(16, 185, 129, 0.015) 1px, transparent 1px),
          radial-gradient(circle at 50% 50%, rgba(16, 185, 129, 0.04), rgba(9, 13, 22, 1))
        `,
        backgroundSize: "32px 32px, 32px 32px, 100% 100%",
      }}
    >
      <div className="pointer-events-none absolute inset-0 bg-gradient-to-b from-transparent via-emerald-500/[0.008] to-transparent bg-[length:100%_6px]" />

      <div className="relative z-10 w-full max-w-xl border border-emerald-500/40 bg-zinc-950 p-6 shadow-xl md:p-8">
        <div className="absolute -top-[1px] -left-[1px] h-2 w-2 border-t border-l border-emerald-400" />
        <div className="absolute -top-[1px] -right-[1px] h-2 w-2 border-t border-r border-emerald-400" />
        <div className="absolute -bottom-[1px] -left-[1px] h-2 w-2 border-b border-l border-emerald-400" />
        <div className="absolute -bottom-[1px] -right-[1px] h-2 w-2 border-b border-r border-emerald-400" />

        {/* Header */}
        <header className="mb-4 border-b border-emerald-500/20 pb-4 text-left">
          <div className="flex items-center justify-between">
            <span className="text-[11px] tracking-wider text-emerald-500/40">
              WSHARE_PROTOCOL // v1.0.0
            </span>
            <span className="h-1.5 w-1.5 rounded-full bg-emerald-500/70" />
          </div>
          <h1 className="mt-2 text-2xl font-bold tracking-wider text-emerald-400">
            &gt; wshare
          </h1>
          <p className="mt-1 text-[11px] text-emerald-600 uppercase tracking-wide">
            Status: System Ready
          </p>
        </header>

        <div className="mb-4 flex border-b border-emerald-500/20 text-xs">
          <button
            onClick={() => setActiveTab("upload")}
            className={`px-4 py-2 border-t border-x transition-colors duration-150 ${
              activeTab === "upload"
                ? "border-emerald-500/30 bg-black text-emerald-400 font-bold"
                : "border-transparent text-emerald-700 hover:text-emerald-500"
            }`}
          >
            [01] UPLOAD
          </button>
          <button
            onClick={() => setActiveTab("download")}
            className={`px-4 py-2 border-t border-x transition-colors duration-150 ${
              activeTab === "download"
                ? "border-emerald-500/30 bg-black text-emerald-400 font-bold"
                : "border-transparent text-emerald-700 hover:text-emerald-500"
            }`}
          >
            [02] DOWNLOAD
          </button>
        </div>

        {activeTab === "upload" ? (
          <div className="relative border border-emerald-500/20 bg-black p-6 text-center">
            <span className="absolute -top-2 left-4 bg-zinc-950 px-2 text-[11px] font-medium text-emerald-400 uppercase tracking-wider">
              File_Entry
            </span>

            <p className="mb-6 text-xs text-emerald-600/90 uppercase tracking-wide">
              Stream system files directly to the local network share.
            </p>

            <div className="flex flex-col sm:flex-row justify-center gap-4">
              <button
                disabled={uploading}
                onClick={() => fileInputRef.current?.click()}
                className="border border-emerald-500/60 bg-transparent px-5 py-2 text-xs font-bold tracking-widest text-emerald-400 transition-colors duration-150 hover:bg-emerald-500 hover:text-black disabled:opacity-30 disabled:pointer-events-none"
              >
                [ LOAD_FILES ]
              </button>
              <button
                disabled={uploading}
                onClick={() => folderInputRef.current?.click()}
                className="border border-emerald-500/20 bg-transparent px-5 py-2 text-xs font-bold tracking-widest text-emerald-600 transition-colors duration-150 hover:border-emerald-500/60 hover:text-emerald-400 disabled:opacity-30 disabled:pointer-events-none"
              >
                [ LOAD_DIRECTORY ]
              </button>
            </div>

            <input
              type="file"
              multiple
              ref={fileInputRef}
              onChange={onFileChange}
              className="hidden"
            />
            <input
              type="file"
              {...({ webkitdirectory: "true", directory: "true" } as any)}
              multiple
              ref={folderInputRef}
              onChange={onFileChange}
              className="hidden"
            />
          </div>
        ) : (
          <div className="relative border border-emerald-500/20 bg-black p-4 text-left">
            <span className="absolute -top-2 left-4 bg-zinc-950 px-2 text-[11px] font-medium text-emerald-400 uppercase tracking-wider">
              Server_Storage_Nodes
            </span>

            <p className="mb-4 text-[11px] text-emerald-600/90 uppercase tracking-wide">
              Select files located inside target download directory workspace.
            </p>

            <div className="max-h-[180px] overflow-y-auto border border-emerald-500/10 divide-y divide-emerald-500/10 scrollbar-thin scrollbar-thumb-emerald-500/20">
              {serverFiles.length === 0 ? (
                <div className="p-4 text-center text-xs text-emerald-700 italic">
                  NO_OBJECTS_FOUND_IN_WORKSPACE_TARGET
                </div>
              ) : (
                serverFiles.map((file, idx) => (
                  <div
                    key={idx}
                    className="flex items-center justify-between p-2.5 text-xs hover:bg-emerald-500/5 transition-colors duration-75 group"
                  >
                    <div className="truncate pr-4">
                      <span className="text-emerald-700 mr-2">
                        [{idx.toString().padStart(2, "0")}]
                      </span>
                      <span className="text-emerald-400 font-medium truncate">
                        {file.name}
                      </span>
                      <span className="block text-[10px] text-emerald-600/60 truncate mt-0.5">
                        {file.path}
                      </span>
                    </div>
                    <div className="flex items-center gap-3 shrink-0">
                      <span className="text-[10px] text-emerald-600">
                        {file.size}
                      </span>
                      <button
                        onClick={() => handleDownloadFile(file)}
                        className="border border-emerald-500/40 bg-zinc-900 px-2 py-1 text-[10px] font-bold text-emerald-400 hover:bg-emerald-500 hover:text-black transition-colors"
                      >
                        GET
                      </button>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        )}

        {/* Progress System */}
        {uploading && (
          <div className="mt-4 border border-amber-500/20 bg-black p-3 text-left">
            <div className="text-[11px] text-amber-500 uppercase tracking-wider mb-1">
              Streaming Segment Payload...
            </div>
            <div className="text-xs font-bold text-amber-400 tracking-wider">
              {renderTerminalProgress()}
            </div>
          </div>
        )}

        {status.text && (
          <div
            className={`mt-4 border p-3 text-left text-xs uppercase tracking-wide ${
              status.type === "success"
                ? "border-emerald-500/30 bg-emerald-950/10 text-emerald-400"
                : status.type === "error"
                  ? "border-rose-500/30 bg-rose-950/10 text-rose-400"
                  : "border-amber-500/30 bg-amber-950/10 text-amber-400"
            }`}
          >
            <span className="font-bold mr-2 text-emerald-500/50">&gt;&gt;</span>
            {status.text}
          </div>
        )}

        <footer className="mt-6 text-center text-[9px] text-emerald-800/60 tracking-widest uppercase">
          // Connection link initialized over standard browser transport loop
        </footer>
      </div>
    </div>
  );
}
