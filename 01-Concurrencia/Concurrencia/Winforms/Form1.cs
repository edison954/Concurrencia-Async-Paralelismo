using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winforms
{
    public partial class Form1 : Form
    {

        private string apiURL;
        private HttpClient httpClient;
        private CancellationTokenSource cancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
            apiURL = "https://localhost:44317";
            httpClient = new HttpClient();
        }

        private async void btnIniciar_Click(object sender, EventArgs e)
        {

            // contexto de sincronizacion
            //Console.WriteLine($"hilo antes del await: {Thread.CurrentThread.ManagedThreadId}");
            //await Task.Delay(500);
            //Console.WriteLine($"hilo despues del await: {Thread.CurrentThread.ManagedThreadId}");

            // await ObtenerSaludo2("Edison");


            ////configureAwait

            //CheckForIllegalCrossThreadCalls = true;

            //btnCancelar.Text = "antes";
            //await Task.Delay(1000).ConfigureAwait(continueOnCapturedContext: false);

            //btnCancelar.Text = "despues";


            // Patron Reintento
            var reintentos = 3;
            var tiempoEspera = 500;

            //for (int i = 0; i < reintentos; i++)
            //{
            //    try
            //    {
            //        // operacion
            //        break;
            //    }
            //    catch (Exception ex)
            //    {
            //        // loggear la excepcion
            //        await Task.Delay(tiempoEspera);                    
            //    }
            //}

            //await Reintentar(ProcesarSaludo);


            //// Patron de reintento ***

            //try
            //{
            //    var contenido = await Reintentar(async () =>
            //    {
            //        using (var respuesta = await httpClient.GetAsync($"{apiURL}/saludos2/edison"))
            //        {
            //            respuesta.EnsureSuccessStatusCode();
            //            return await respuesta.Content.ReadAsStringAsync();
            //        }
            //    });

            //    Console.WriteLine(contenido);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Excepcion atrapada");                
            //}

            //// End Patron de reintento ***

            // Patron solo una tarea
            // solo quiero que se ejecute una tarea y canccelar las demas

            //1.
            //cancellationTokenSource = new CancellationTokenSource();
            //var token = cancellationTokenSource.Token;

            var nombres = new string[] { "Felipe", "Claudia", "Antonio", "Edison" };

            //var tareasHTTP = nombres.Select(x => ObtenerSaludo3(x, token));
            //var tarea = await Task.WhenAny(tareasHTTP);
            //var contenido = await tarea;
            //Console.WriteLine(contenido.ToUpper());
            //cancellationTokenSource.Cancel();


            //2.
            //var tareasHTTP = nombres.Select(x =>
            //{
            //    Func<CancellationToken, Task<string>> funcion = (ct) => ObtenerSaludo3(x, ct);
            //    return funcion;

            //});

            //var contenido = await EjecutarUno(tareasHTTP);
            //Console.WriteLine(contenido.ToUpper());

            //3.
            //var contenido = await EjecutarUno(
            //        (ct) => ObtenerSaludo3("Edison", ct),
            //        (ct) => ObtenerAdios("Edison", ct)
            //    );
            //Console.WriteLine(contenido.ToUpper());


            // End Patron solo una tarea


            //// Controlar el resultado de la tarea con TaskCompletionSource
            //var tarea = EvaluarValor(txtInput.Text);
            //Console.WriteLine("Inicio");
            //Console.WriteLine($"Is completed: {tarea.IsCompleted}");
            //Console.WriteLine($"Is canceled: {tarea.IsCanceled}");
            //Console.WriteLine($"Is faulted: {tarea.IsFaulted}");

            //try
            //{
            //    await tarea;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Excepcion: {ex.Message}");
            //}
            //Console.WriteLine("fin");
            //Console.WriteLine("");


            // Cancelando tareas no cancelables  (metodos asincronos que no reciben un token de cancelacion por ello no se puede usar el cancelationtoken)
            // util cuando no se quiere programar un timeout, sino que queremos una tarea que no hace nada pero queremos poderla cancelar

            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                var resultado = await Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    return 7;
                }).WithCancellation(cancellationTokenSource.Token);
                Console.WriteLine(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally {

                cancellationTokenSource.Dispose();
            }





            return;

            //cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));


            loadingGif.Visible = true;
            pgProcesamiento.Visible = true;
            var reportarProgreso = new Progress<int>(ReportarProgresoTarjetas);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var tarjetas = await ObtenerTarjetasDeCredito(20, cancellationTokenSource.Token);
                await ProcesarTarjetas(tarjetas, reportarProgreso, cancellationTokenSource.Token);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("La operacion ha sido cancelada");
            }

            MessageBox.Show($"Operación finalizada en {stopwatch.ElapsedMilliseconds / 1000.0} segundos");
            loadingGif.Visible = false;
            pgProcesamiento.Visible = false;
            pgProcesamiento.Value = 0;
            // ...
        }

        public Task EvaluarValor(string valor) 
        {
            var tcs = new TaskCompletionSource<object>
                (TaskCreationOptions.RunContinuationsAsynchronously);

            if (valor == "1")
            {
                tcs.SetResult(null);
            }
            else if (valor == "2")
            {
                tcs.SetCanceled();
            }
            else {
                tcs.SetException(new ApplicationException($"Valor inválido: {valor}"));
            }

            return tcs.Task;
        
        }



        private async Task<T> EjecutarUno<T>(params Func<CancellationToken, Task<T>>[] funciones)
        {

            var cts = new CancellationTokenSource();
            var tareas = funciones.Select(funcion => funcion(cts.Token));
            var tarea = await Task.WhenAny(tareas);
            cts.Cancel();
            return await tarea;

        }

        private async Task<T> EjecutarUno<T>(IEnumerable<Func<CancellationToken, Task<T>>> funciones) 
        {

            var cts = new CancellationTokenSource();
            var tareas = funciones.Select(funcion => funcion(cts.Token));
            var tarea = await Task.WhenAny(tareas);
            cts.Cancel();
            return await tarea;

        }



        private async Task ProcesarSaludo() {

            using (var respuesta = await httpClient.GetAsync($"{apiURL}/saludos2/edison"))
            {
                respuesta.EnsureSuccessStatusCode();
                var contenido = await respuesta.Content.ReadAsStringAsync();
                Console.WriteLine(contenido);
            }
        }

        private async Task Reintentar(Func<Task> f, int reintentos = 3, int tiempoEspera = 500)
        {
            for (int i = 0; i < reintentos; i++)
            {
                try
                {
                    await f();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.Delay(tiempoEspera);
                }
            }
        }

        // para retornar el resultado, incluso la excepcion.
        private async Task<T> Reintentar<T>(Func<Task<T>> f, int reintentos = 3, int tiempoEspera = 500)
        {
            for (int i = 0; i < reintentos -1; i++)
            {
                try
                {
                    return await f();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.Delay(tiempoEspera);
                }
            }
            return await f();
        }



        private void ReportarProgresoTarjetas(int porcentaje)
        {
            pgProcesamiento.Value = porcentaje;
        }



        private Task ProcesarTarjetasMock(List<string> tarjetas, IProgress<int> progress = null, CancellationToken cancellationToken = default)
        {
            // ...
            return Task.CompletedTask;
        }



        private async Task ProcesarTarjetas(List<string> tarjetas, IProgress<int> progress = null, CancellationToken cancellationToken = default)
        {

            using var semaforo = new SemaphoreSlim(2);

            var tareas = new List<Task<HttpResponseMessage>>();

            var indice = 0;

            tareas = tarjetas.Select(async tarjeta =>
            {
                var json = JsonConvert.SerializeObject(tarjeta);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await semaforo.WaitAsync();
                try
                {
                    var tareaInterna = await httpClient.PostAsync($"{apiURL}/tarjetas", content, cancellationToken);

                    //if (progress != null)
                    //{
                    //    indice++;
                    //    var porcentaje = (double)indice / tarjetas.Count;
                    //    porcentaje = porcentaje * 100;
                    //    var porcentajeInt = (int)Math.Round(porcentaje, 0);
                    //    progress.Report(porcentajeInt);
                    //}

                    return tareaInterna;
                }
                catch (Exception)
                {
                    throw;
                }
                finally {
                    semaforo.Release();
                }

            }).ToList();

            var respuestasTareas = Task.WhenAll(tareas);

            if(progress != null)
            {
                while (await Task.WhenAny(respuestasTareas, Task.Delay(1000)) != respuestasTareas) 
                {
                    // ejecutar esta pieza de codigo cada 1 segundo siempre y cuando el lsitado de tareas no se haya concluido
                    var tareasCompletadas = tareas.Where(x => x.IsCompleted).Count();
                    var porcentaje = (double)tareasCompletadas / tarjetas.Count;
                    porcentaje = porcentaje * 100;
                    var porcentajeInt = (int)Math.Round(porcentaje, 0);
                    progress.Report(porcentajeInt);
                }

            }

            // no importa que se haga await a la misma tarea, ya que si la tarea ya esta completada no la vuelve a ejecutar.
            var respuestas = await respuestasTareas;


            var tarjetasRechazadas = new List<string>();

            foreach (var respuesta in respuestas)
            {
                var contenido = await respuesta.Content.ReadAsStringAsync();
                var respuestaTarjeta = JsonConvert.DeserializeObject<RespuestaTarjeta>(contenido);
                if (!respuestaTarjeta.Aprobada) {
                    tarjetasRechazadas.Add(respuestaTarjeta.Tarjeta);
                }
            }

            foreach (var tarjeta in tarjetasRechazadas)
            {
                Console.WriteLine(tarjeta);
            }

        }


        private  Task<List<string>> ObtenerTarjetasDeCreditoMock(int cantidadDeTarjetas, CancellationToken cancellationToken = default)
        {
            var tarjetas = new List<string>();
            tarjetas.Add("00000000000001");

            return Task.FromResult(tarjetas);
        }

        private Task ObtenerTareaConError() {
            return Task.FromException(new ApplicationException());
        }

        private Task ObtenerTareaCancelada()
        {
            cancellationTokenSource = new CancellationTokenSource();
            return Task.FromCanceled(cancellationTokenSource.Token);
        }




        private async Task<List<string>> ObtenerTarjetasDeCredito(int cantidadDeTarjetas, CancellationToken cancellationToken = default)
        {
            return await Task.Run(async () =>
            {

                var tarjetas = new List<string>();
                for (int i = 0; i < cantidadDeTarjetas; i++)
                {

                    // simular procesamiento largo
                    await Task.Delay(1000);

                    // 0000000000001
                    // 0000000000002
                    tarjetas.Add(i.ToString().PadLeft(16, '0'));


                    Console.WriteLine($"Han sido generadas {tarjetas.Count} tarjetas");

                    // variable que indica si se ha solicitado la cancelaccion del token
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }

                }
                return tarjetas;

            });
        }



        private async Task Esperar() {
            await Task.Delay(TimeSpan.FromSeconds(0));
        }

        private async Task<string> ObtenerSaludo(string nombre)
        {
            using (var respuesta = await httpClient.GetAsync($"{apiURL}/saludos/{nombre}"))
            {
                respuesta.EnsureSuccessStatusCode();
                var saludo = await respuesta.Content.ReadAsStringAsync();
                return saludo;
            }
        }


        private async Task<string> ObtenerAdios(string nombre, CancellationToken cancellationToken)
        {
            using (var respuesta = await httpClient.GetAsync($"{apiURL}/saludos/adios/{nombre}", cancellationToken))
            {
                var saludo = await respuesta.Content.ReadAsStringAsync();
                Console.WriteLine(saludo);
                return saludo;
            }
        }



        private async Task<string> ObtenerSaludo3(string nombre, CancellationToken cancellationToken)
        {
            using (var respuesta = await httpClient.GetAsync($"{apiURL}/saludos/delay/{nombre}", cancellationToken))
            {
                var saludo = await respuesta.Content.ReadAsStringAsync();
                Console.WriteLine(saludo);
                return saludo;
            }
        }




        private async Task<string> ObtenerSaludo2(string nombre)
        {
            using (var respuesta = await httpClient.GetAsync($"{apiURL}/saludos/delay/{nombre}"))
            {
                var saludo = await respuesta.Content.ReadAsStringAsync();
                return saludo;
            }
        }




        private void btnCancelar_Click(object sender, EventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }
    }
}
