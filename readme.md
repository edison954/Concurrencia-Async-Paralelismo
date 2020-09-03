Concurrencia, Async y Paralelismo

Concurrencia: hacer varias cosas al mismo tiempo
Paralelismo: hacer las cosas de manera simultanea (utilizando hilos) usa multihilos y este es una forma de concurrencia

Eficiencia: velocidad, uso de recursos

programacion asincrona: aquella que utiliza los hilos de manera eficiente
(evitar el bloqueo de hilos)

Paralelismo:  ahorrar tiempo usando los recursos del pc

_=> segun curso: no es comun, recomendable usar paralelismo en ambientes web aspnet o aspnet.core
en lugar de ello recomienda usar backgloundservices o tecnologias serverless como azure functions

libreiras PTL
PLINQ

Paralelismo de datos  (ej: filtrado de elementos)
Paralelismo de tareas (tareas independientes en paralelo)
No siempre debemos usar paralelismo

Asincrona
-Eviar bloquear un hilo
dos beneficios:
Escalabilidad vertical e interfaz de usuario que no se congela

lo normal es que se usa cuando nos vamos a comunicar con sistemas externos.

Operacuibes I/O (input/Output)
ej:
    llamado webservice
    a bd
    a un sistema de archivos


operaciones de cpu
ej:
    ordernar un arreglo
    obtener la inversa de una matriz

I/O vs operacion CPU    --> determinan que usar, si paralelo o async

Usar:
I/O  -->  programacion asyncrona
CPU  -->  usar paralelo

conceptos:
Secuncial : instrucciones una a la vez.
Concurrencia : varias cosas al mismo tiempo
Multihilos : capacidad de usar varios hilos (no implica paralelismo: ej: cuando un pc no tiene varios nucleos)
Paralelismo : correr varios hilos de manera simultanea (necesita un procesador con varios nucleos) (es una forma de multihilos)
Multitarea : tener varias tareas ejecutandose no necesariamente al mismo tiempo  (paralelismo persivido pero no real)

Determinismo vs no determinismo

Determinismo
se predicen los valores de salida a partir de los valores de entrada
ej: suma(int a, int b) => a  + b;           (2 + 5)  = 7

No determinismo
cuando no se pueden predecir los valores de salida a partir de los valores de entrada
ej: Random, Paralelismo (ya que no se predice el orden de ejecucion de los hilos, si se necesita orden de llegada entonces evaluar no usar paralelismo)

Concurrencia:
async - await
01-concurrencia

en lugar de 
// Thread.Sleep(5000);

usar
await Task.Delay(TimeSpan.FromSeconds(5));


se retorna
Task, Task<T>
ValueTask o ValueTask<T> 

async void    --> evitarlos, unicamente usarlos posiblemente con void para manejadores de eventos
ej: private async void btnIniciar_Click(object sender, EventArgs e)

      private async Task esperar()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }


OJO: si no se hace un await de un task, la posible excepcion queda oculta y no se retorna a la capa superior

La excepcion de un Task unicamente se arroja cuando se hace el await,. sino queda oculta dentro del task

            try
            {
                var saludo = await ObtenerSaludo(nombre);
                MessageBox.Show(saludo); ;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message);
            }



[ThreadStatic]  -- > permite tener una copia por hilo de una variable (unico para cada hilo)

[ThreadStatic]
private static Random _local;

// Crear stopwatch para revisar tiempo de ejecucion

            loadingGif.Visible = true;
            var tarjetas = ObtenerTarjetasDeCredito(5);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // ..... 
            MessageBox.Show($"Operación finalizada en {stopwatch.ElapsedMilliseconds / 1000.0} segundos");

No se procesan en orden:

Tarjeta 0000000000000003 procesada
Tarjeta 0000000000000002 procesada
Tarjeta 0000000000000001 procesada
Tarjeta 0000000000000000 procesada
Tarjeta 0000000000000004 procesada


Throttling:   ---> controlar el numero de peticiones que se pueden manejar
                        eJ: controlar el uso de las peticiones http

para ello se usan semafors

using var semaforo = new SemaphoreSlim(4000);               --> using  en C# 8   (FW 4.8)   (ej va a permitir correr las tareas de 4000 en 4000)
en el csproj poner

  <PropertyGroup>
    <LangVersion>8.0</LangVersion>

ej: se procesan de 3 en tres


        private async Task ProcesarTarjetas(List<string> tarjetas)
        {

            using var semaforo = new SemaphoreSlim(3);

            var tareas = new List<Task<HttpResponseMessage>>();
            tareas = tarjetas.Select(async tarjeta =>
            {
                var json = JsonConvert.SerializeObject(tarjeta);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await semaforo.WaitAsync();
                try
                {
                    return await httpClient.PostAsync($"{apiURL}/tarjetas", content);
                }
                catch (Exception)
                {
                    throw;
                }
                finally {
                    semaforo.Release();
                }

            }).ToList();

            await Task.WhenAll(tareas);
        }



SemaphoreSlim (controlar la cantidad de tareas simultaneas que van a ejecutarse en el servidor)        

Obtener el resultado de las tareas y procesarlo   (en el ejemplo tarjetas rechazadas)


        private async void btnIniciar_Click(object sender, EventArgs e)
        {
            loadingGif.Visible = true;
            var tarjetas = await ObtenerTarjetasDeCredito(2500);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                await ProcesarTarjetas(tarjetas);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message);
            }

            MessageBox.Show($"Operación finalizada en {stopwatch.ElapsedMilliseconds / 1000.0} segundos");
            loadingGif.Visible = false;
            // ...
        }

        private async Task ProcesarTarjetas(List<string> tarjetas)
        {

            using var semaforo = new SemaphoreSlim(1000);

            var tareas = new List<Task<HttpResponseMessage>>();
            tareas = tarjetas.Select(async tarjeta =>
            {
                var json = JsonConvert.SerializeObject(tarjeta);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await semaforo.WaitAsync();
                try
                {
                    return await httpClient.PostAsync($"{apiURL}/tarjetas", content);
                }
                catch (Exception)
                {
                    throw;
                }
                finally {
                    semaforo.Release();
                }

            }).ToList();

            var respuestas = await Task.WhenAll(tareas);

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

        private async Task<List<string>> ObtenerTarjetasDeCredito(int cantidadDeTarjetas)
        {
            return await Task.Run(() =>
            {

                var tarjetas = new List<string>();
                for (int i = 0; i < cantidadDeTarjetas; i++)
                {
                    // 0000000000001
                    // 0000000000002
                    tarjetas.Add(i.ToString().PadLeft(16, '0'));
                }
                return tarjetas;

            });
        }


------------------------------ -----------------------------------------
Reprotar progreso de una tarea

Con IProgress ( es no recomendable para millones de peticiones para ello usar task.whenany)

con IProgress:

        private async void btnIniciar_Click(object sender, EventArgs e)
        {
            loadingGif.Visible = true;
            pgProcesamiento.Visible = true;
            var reportarProgreso = new Progress<int>(ReportarProgresoTarjetas);

            var tarjetas = await ObtenerTarjetasDeCredito(20);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                await ProcesarTarjetas(tarjetas, reportarProgreso);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message);
            }

            MessageBox.Show($"Operación finalizada en {stopwatch.ElapsedMilliseconds / 1000.0} segundos");
            loadingGif.Visible = false;
            pgProcesamiento.Visible = false;
            // ...
        }

        private void ReportarProgresoTarjetas(int porcentaje)
        {
            pgProcesamiento.Value = porcentaje;
        }


        private async Task ProcesarTarjetas(List<string> tarjetas, IProgress<int> progress = null)
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
                    var tareaInterna = await httpClient.PostAsync($"{apiURL}/tarjetas", content);

                    if (progress != null)
                    {
                        indice++;
                        var porcentaje = (double)indice / tarjetas.Count;
                        porcentaje = porcentaje * 100;
                        var porcentajeInt = (int)Math.Round(porcentaje, 0);
                        progress.Report(porcentajeInt);
                    }

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

            var respuestas = await Task.WhenAll(tareas);

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

        private async Task<List<string>> ObtenerTarjetasDeCredito(int cantidadDeTarjetas)
        {
            return await Task.Run(() =>
            {

                var tarjetas = new List<string>();
                for (int i = 0; i < cantidadDeTarjetas; i++)
                {
                    // 0000000000001
                    // 0000000000002
                    tarjetas.Add(i.ToString().PadLeft(16, '0'));
                }
                return tarjetas;

            });
        }


---------------------------------------------------------------------------------------------------

Reportar progreso con Task.WhenAny()                --> reportar progreso por intervalos (ej cada segundo)
Reportando progreso cada segundo



        private async void btnIniciar_Click(object sender, EventArgs e)
        {
            loadingGif.Visible = true;
            pgProcesamiento.Visible = true;
            var reportarProgreso = new Progress<int>(ReportarProgresoTarjetas);

            var tarjetas = await ObtenerTarjetasDeCredito(20);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                await ProcesarTarjetas(tarjetas, reportarProgreso);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message);
            }

            MessageBox.Show($"Operación finalizada en {stopwatch.ElapsedMilliseconds / 1000.0} segundos");
            loadingGif.Visible = false;
            pgProcesamiento.Visible = false;
            // ...
        }

        private void ReportarProgresoTarjetas(int porcentaje)
        {
            pgProcesamiento.Value = porcentaje;
        }


        private async Task ProcesarTarjetas(List<string> tarjetas, IProgress<int> progress = null)
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
                    var tareaInterna = await httpClient.PostAsync($"{apiURL}/tarjetas", content);

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

        private async Task<List<string>> ObtenerTarjetasDeCredito(int cantidadDeTarjetas)
        {
            return await Task.Run(() =>
            {

                var tarjetas = new List<string>();
                for (int i = 0; i < cantidadDeTarjetas; i++)
                {
                    // 0000000000001
                    // 0000000000002
                    tarjetas.Add(i.ToString().PadLeft(16, '0'));
                }
                return tarjetas;

            });
        }



---------------------------------------------------------------------------------------

Cancelar tareas  CancellationToken


        private async void btnIniciar_Click(object sender, EventArgs e)
        {

            cancellationTokenSource = new CancellationTokenSource();


            loadingGif.Visible = true;
            pgProcesamiento.Visible = true;
            var reportarProgreso = new Progress<int>(ReportarProgresoTarjetas);

            var tarjetas = await ObtenerTarjetasDeCredito(20);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
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

        private void ReportarProgresoTarjetas(int porcentaje)
        {
            pgProcesamiento.Value = porcentaje;
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

        private async Task<List<string>> ObtenerTarjetasDeCredito(int cantidadDeTarjetas)
        {
            return await Task.Run(() =>
            {

                var tarjetas = new List<string>();
                for (int i = 0; i < cantidadDeTarjetas; i++)
                {
                    // 0000000000001
                    // 0000000000002
                    tarjetas.Add(i.ToString().PadLeft(16, '0'));
                }
                return tarjetas;

            });
        }



        private async Task Esperar() {
            await Task.Delay(TimeSpan.FromSeconds(0));
        }

        private async Task<string> ObtenerSaludo(string nombre)
        {
            using (var respuesta = await httpClient.GetAsync($"{apiURL}/saludos2/{nombre}"))
            {
                respuesta.EnsureSuccessStatusCode();
                var saludo = await respuesta.Content.ReadAsStringAsync();
                return saludo;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }
    }



-------------------------------------------------------------------------------
Cancelando Bucles






