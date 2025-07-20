// -------------------------------------------------------------------
// Programmer       : Ebrahim Shafiei (EbraSha)
// Email            : Prof.Shafiei@Gmail.com
// -------------------------------------------------------------------

using Abdal_Security_Group_App.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using static Telerik.WinControls.VistaAeroTheme;

namespace Abdal_Security_Group_App
{
    public class ProtoMonitor
    {
        private readonly Main _form;
        private readonly Monitor _monitor_form;
        private HttpListener _listener;
        private Thread _listenerThread;
        private CancellationTokenSource _cts;
        private int _port;
        private AbdalSoundPlayer ab_player = new AbdalSoundPlayer();
        public ProtoMonitor(Main form, Monitor monitor_form)
        {
            _form = form;
            _monitor_form = monitor_form;
        }

        



        public void Start()
        {
            ProcessKiller("A4iProtoC");

            _port = FindFreePort();
            _cts = new CancellationTokenSource();

            _listenerThread = new Thread(() => StartListener(_cts.Token));
            _listenerThread.IsBackground = true;
            _listenerThread.Start();

            StartProcessWithArgs();
        }

        public void Stop()
        {
            try
            {
                _cts?.Cancel();

                if (_listener != null)
                {
                    if (_listener.IsListening)
                        _listener.Stop();

                    _listener.Close();
                    _listener = null;
                }

                _cts = null;
                _listenerThread = null;

                ProcessKiller("A4iProtoC");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Stop error: " + ex.Message);
            }
        }

        public static void ProcessKiller(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try { process.Kill(true); } catch { /* ignore */ }
            }
        }

        private int FindFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private void StartProcessWithArgs()
        {
            var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "A4iProtoC.exe");
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

            if (!File.Exists(exePath))
            {
                MessageBox.Show("A4iProtoC.exe not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"-c \"{configPath}\" -p {_port}",
                WorkingDirectory = Path.GetDirectoryName(exePath),
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start process: " + ex.Message);
            }
        }

        private void StartListener(CancellationToken token)
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://127.0.0.1:{_port}/log/");
                _listener.Start();

                while (_listener.IsListening && !token.IsCancellationRequested)
                {
                    try
                    {
                        var context = _listener.GetContext();
                        ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
                    }
                    catch (HttpListenerException) { break; }
                    catch (ObjectDisposedException) { break; }
                    catch (Exception ex)
                    {
                       // Debug.WriteLine("Unexpected listener error: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                // نمایش نده اگر فقط به خاطر توقف باشه
                if (!token.IsCancellationRequested)
                    MessageBox.Show("Listener error: " + ex.Message);
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string body = reader.ReadToEnd();

                var log = JsonSerializer.Deserialize<AbdalLog>(body);

                _form.Invoke((MethodInvoker)delegate
                {
                    _form.lbBytesSent.Text = log.bytes_sent.ToString("N0") + "";
                    _form.lbBytesReceived.Text = log.bytes_received.ToString("N0") + "";

                    if (!string.IsNullOrWhiteSpace(log.message))
                    {
                       // _form.listBoxLog.Items.Insert(0, $"[{log.level}] {log.message}");
                        if (log.message.Contains("SSH connection established and monitoring"))
                        {
                            ab_player.sPlayer("done");
                            _form.ConnectTunnelBtn.BackgroundImage = _form.imageListBtns.Images["suc-btn"];
                            _form.ConnectionStatus = "connected";
                            _form.StartConnectingTimerAsync();
                        }

                        if (log.message.Contains("SSH connect failed")){
                            _form.ConnectionStatus = "connection_error";
                            _form.ConnectTunnelBtn.BackgroundImage = _form.imageListBtns.Images["danger-btn"];
                            _form.StopConnectingTimer();
                        }

                        /**
                        _form.listBoxLog.TopIndex = _form.listBoxLog.Items.Count - 1;
                        if (_form.listBoxLog.Items.Count > 100)
                            _form.listBoxLog.Items.RemoveAt(_form.listBoxLog.Items.Count - 1);
                        **/
                    }
                });

                _monitor_form.Invoke((MethodInvoker)delegate
                {
                    _monitor_form.listBoxLog.Items.Insert(0, $"[{log.level}] {log.message}");

                    _monitor_form.listBoxLog.TopIndex = _monitor_form.listBoxLog.Items.Count - 1;
                    if (_monitor_form.listBoxLog.Items.Count > 35)
                        _monitor_form.listBoxLog.Items.RemoveAt(_monitor_form.listBoxLog.Items.Count - 1);
                });

                context.Response.StatusCode = 200;
                context.Response.Close();
            }
            catch
            {
                try { context.Response.StatusCode = 500; context.Response.Close(); } catch { }
            }
        }

        private class AbdalLog
        {
            public string username { get; set; }
            public string ip { get; set; }
            public long bytes_sent { get; set; }
            public long bytes_received { get; set; }
            public long total_bytes { get; set; }
            public string timestamp { get; set; }
            public string message { get; set; }
            public string level { get; set; }
        }
    }
}
