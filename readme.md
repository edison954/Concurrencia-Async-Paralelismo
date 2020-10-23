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



        private async void btnIniciar_Click(object sender, EventArgs e)
        {

            cancellationTokenSource = new CancellationTokenSource();


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


--------------------------------------------------------------------------
Cancelando tareas por tiempo - timeout
ej: cancelar la operacion si esta tardando mas de 3s

cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));


--------------------------------------------------------------------------

Creando tareas ya terminadas  Task.FromResult           ---> por lo general para pruebas unitarias

        private Task ProcesarTarjetasMock(List<string> tarjetas, IProgress<int> progress = null, CancellationToken cancellationToken = default)
        {
            // ...
            return Task.CompletedTask;
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


---------------------------------------------------------------------

Contexto de sincronizacion   (no todos los hilos son iguales)

(en ocaciones se necesita que al suspender la ejecucion del hilo con el await, resumamos la ejecucion del metodo en el mismo hilo original)


            // contexto de sincronizacion  (en aplicacion winforms es el mismo hilo)
            Console.WriteLine($"hilo antes del await: {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(500);
            Console.WriteLine($"hilo despues del await: {Thread.CurrentThread.ManagedThreadId}");

en netcore no se tiene el contexto de sincronizacion por lo cual son hilos diferentes

        [HttpGet("delay/{nombre}")]
        public async Task<ActionResult<string>> ObtenerSaludoConDelay(string nombre)
        {
            Console.WriteLine($"hilo antes del await: {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(500);
            Console.WriteLine($"hilo después del await: {Thread.CurrentThread.ManagedThreadId}");

            var esperar = RandomGen.NextDouble() * 10 + 1;
            await Task.Delay((int)esperar * 1000);
            return $"Hola, {nombre}!";
        }

--------------------------------------------------------

ConfigureAwait(false)           --> para que despues del await se resuma la ejecucion en otro hilo
                Se usa cuando no se necesita el hilo original despues de ejecutar un await  
                (ejemplo libreira con hilo asincrono, por performance ya que toma mas tiempo sincronizar el hilo.)

            btnCancelar.Text = "antes";
            await Task.Delay(1000);

            btnCancelar.Text = "despues";

////

            CheckForIllegalCrossThreadCalls = true;                             --> check para verificar si se hacen llamadas ilegales entre hilos

            btnCancelar.Text = "antes";
            await Task.Delay(1000).ConfigureAwait(continueOnCapturedContext: false);

            btnCancelar.Text = "despues";

-----------------------------------------------------------

Patron de reintento (con tiempo de espera)

primer intento: (no sofisticado)

            var reintentos = 3;
            var tiempoEspera = 500;

            for (int i = 0; i < reintentos; i++)
            {
                try
                {
                    // operacion
                    break;
                }
                catch (Exception ex)
                {
                    // loggear la excepcion
                    await Task.Delay(tiempoEspera);                    
                }
            }


metodo sofisticado  (patron de reintento)            


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


            await Reintentar( async () => 
            {
                using (var respuesta = await httpClient.GetAsync($"{apiURL}/saludos2/edison")) 
                {
                    respuesta.EnsureSuccessStatusCode();
                    var contenido = await respuesta.Content.ReadAsStringAsync();
                    Console.WriteLine(contenido);
                } 
            });

---> seria igual a: 

        await Reintentar(ProcesarSaludo);

        private async Task ProcesarSaludo() {

            using (var respuesta = await httpClient.GetAsync($"{apiURL}/saludos2/edison"))
            {
                respuesta.EnsureSuccessStatusCode();
                var contenido = await respuesta.Content.ReadAsStringAsync();
                Console.WriteLine(contenido);
            }
        }

------> para atrapar la excepcion luego de los reintentos

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

           try
            {
                var contenido = await Reintentar(async () =>
                {
                    using (var respuesta = await httpClient.GetAsync($"{apiURL}/saludos2/edison"))
                    {
                        respuesta.EnsureSuccessStatusCode();
                        return await respuesta.Content.ReadAsStringAsync();
                    }
                });

                Console.WriteLine(contenido);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Excepcion atrapada");                
            }


------------------------------------------------------------
Patron una sola tarea               -->> poner a ejecutar las tareas y cancelarlas cuando la primera termine

            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            var nombres = new string[] { "Felipe", "Claudia", "Antonio", "Edison" };

            var tareasHTTP = nombres.Select(x => ObtenerSaludo3(x, token));
            var tarea = await Task.WhenAny(tareasHTTP);
            var contenido = await tarea;
            Console.WriteLine(contenido.ToUpper());
            cancellationTokenSource.Cancel();


        private async Task<string> ObtenerSaludo3(string nombre, CancellationToken cancellationToken)
        {
            using (var respuesta = await httpClient.GetAsync($"{apiURL}/saludos/delay/{nombre}", cancellationToken))
            {
                var saludo = await respuesta.Content.ReadAsStringAsync();
                Console.WriteLine(saludo);
                return saludo;
            }
        }


 eso mismo con el patron seria:


             var tareasHTTP = nombres.Select(x =>
            {
                Func<CancellationToken, Task<string>> funcion = (ct) => ObtenerSaludo3(x, ct);
                return funcion;

            });

            var contenido = await EjecutarUno(tareasHTTP);
            Console.WriteLine(contenido.ToUpper());


        private async Task<T> EjecutarUno<T>(IEnumerable<Func<CancellationToken, Task<T>>> funciones) 
        {

            var cts = new CancellationTokenSource();
            var tareas = funciones.Select(funcion => funcion(cts.Token));
            var tarea = await Task.WhenAny(tareas);
            cts.Cancel();
            return await tarea;

        }

otra forma: (si se tienen diferentes funciones a ejecutar)


            var contenido = await EjecutarUno(
                    (ct) => ObtenerSaludo3("Edison", ct),
                    (ct) => ObtenerAdios("Edison", ct)
                );
            Console.WriteLine(contenido.ToUpper());

        private async Task<T> EjecutarUno<T>(params Func<CancellationToken, Task<T>>[] funciones)
        {

            var cts = new CancellationTokenSource();
            var tareas = funciones.Select(funcion => funcion(cts.Token));
            var tarea = await Task.WhenAny(tareas);
            cts.Cancel();
            return await tarea;

        }


-------------------------------------------------------------------

Controlar resultado de la tarea con TaskCompletionSource
podemos controlar tareas pero somos nosotros los que establecemos el estado (exitoso cancelado, excepcion)


            // Controlar el resultado de la tarea con TaskCompletionSource
            var tarea = EvaluarValor(txtInput.Text);
            Console.WriteLine("Inicio");
            Console.WriteLine($"Is completed: {tarea.IsCompleted}");
            Console.WriteLine($"Is canceled: {tarea.IsCanceled}");
            Console.WriteLine($"Is faulted: {tarea.IsFaulted}");

            try
            {
                await tarea;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepcion: {ex.Message}");
            }
            Console.WriteLine("fin");
            Console.WriteLine("");


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

----------------------------------------------------------
Cualquier tarea no cancelable es cancelable
Cancelando tareas no cancelables  (metodos asincronos que no reciben un token de cancelacion por ello no se puede usar el cancelationtoken)
util cuando no se quiere programar un timeout, sino que queremos una tarea que no hace nada pero queremos poderla cancelar


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


    public static class TaskExtensionMethods
    {

        public static async Task<T> WithCancellation<T>(this Task<T> task,
            CancellationToken cancellationToken)
        {

            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (cancellationToken.Register(state =>
            {
                ((TaskCompletionSource<object>)state).TrySetResult(null);
            }, tcs)) 
            {
                var tareaResultante = await Task.WhenAny(task, tcs.Task);
                if (tareaResultante == tcs.Task) 
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                return await task;

            }
        
        }

    }
}


----------------------------------------------------------------
ValueTask
la mision de ValueTask es de performance
escenarios de alta demanda
ValueTask es un struct   (tipo valor)

usar cuando:
 lo mas probable del resultado de la operacion sea sincrono
 cuando la operacion se invoke muy frecuente que el costo de usar task o task<t>  sea importante
 (en escenarios de alto rendimiento)
----------------------------------------------------------------

Steam Asyncrono

uso de IEnumerable
iteraciones sobre un tipo

            var nombres = new List<string>() {  "Edison", "Andrea"};
            foreach (var nombre in nombres)
            {
                Console.WriteLine(nombre);
            }


yield: para que la lista se entregue de uno en uno
permite generar de uno en uno los valores de un iterable

        private IEnumerable<string> GenerarNombres()
        {
            yield return "Edison";
            yield return "Andrea";            
        }


            foreach (var nombre in GenerarNombres())
            {
                Console.WriteLine(nombre);
            }


----

Stream asyncronos, para poder iterar el tipo task  (instalar paquete de nuget para poder usar IAsyncEnumerable)

            await foreach (var nombre in GenerarNombres())
            {
                Console.WriteLine(nombre);
                break;
            }


        private async IAsyncEnumerable<string> GenerarNombres()
        {
            yield return "Edison";
            await Task.Delay(2000);
            yield return "Andrea";            
        }

-- Tres formas de cancelar el stream asincrono

    1. break            

            await foreach (var nombre in GenerarNombres())
            {
                Console.WriteLine(nombre);
                break;
            }


    2. token de cancelacion


        private async IAsyncEnumerable<string> GenerarNombres(CancellationToken token = default)
        {
            yield return "Edison";
            await Task.Delay(2000, token);
            yield return "Andrea";
            await Task.Delay(2000, token);
            yield return "Paola";
        }    

            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await foreach (var nombre in GenerarNombres())
                {
                    Console.WriteLine(nombre);

                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Operacion cancelada");
            }
            finally {
                cancellationTokenSource?.Dispose();
            }

            Console.WriteLine("fin");


    3. 


        private async IAsyncEnumerable<string> GenerarNombres([EnumeratorCancellation]  CancellationToken token = default)
        {
            yield return "Edison";
            await Task.Delay(2000, token);
            yield return "Andrea";
            await Task.Delay(2000, token);
            yield return "Paola";
        }

        private async Task ProcesarNombres(IAsyncEnumerable<string> nombresEnumerable)
        {

            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await foreach (var nombre  in nombresEnumerable.WithCancellation(cancellationTokenSource.Token))
                {
                    Console.WriteLine(nombre);
                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Operacion cancelada");
            }
            finally
            {
                cancellationTokenSource?.Dispose();
            }
        }        


            var nombresEnumerable = GenerarNombres();
            await ProcesarNombres(nombresEnumerable);
            
            Console.WriteLine("fin");

            cancellationTokenSource = null;


---------------------------------------------------------------

ANTIPATRONES

Sincrono dentro de Asyncrono            (producen deadlock)
Asyncrono detro de sincrono
StarNew
async void                      (puede hacer colapsar toda una aplicacion web)
Dispose del CancellationTokenSource
Dispose Asincrono de Streams



Sincrono dentro de Asyncrono   (sync-over-async)

        private async Task<string> ObtenerValor()
        {
            await Task.Delay(1000);
            return "Edison";
        }

        //asincrono
        private async void btnIniciar_Click(object sender, EventArgs e)
        {

            //sincrono
            var valor = ObtenerValor().Result;              //esto bloquea, esta mal
            Console.WriteLine(valor);

            var a = 2 + 2;  //esto no bloquea, esta bn            
        }

        

-- evitar Task.Factory.StartNew
preferir esto:
 await Task.Run(() => {});

y no esto:

 await Task.Factory.StartNew(() => {}, 
  CancellationToken.None,
  TaskCreationOptions.DenyChildAttach,
  TaskScheduler.Default
  );


ejemplo

            var resultadoStartNew = await Task.Factory.StartNew(async () => {
                await Task.Delay(1000);
                return 7;
            });

            var resultadoRun = await Task.Run(async () =>
            {
                await Task.Delay(1000);
                return 7;
            });

            Console.WriteLine($"Resultado StartNew: {resultadoStartNew}");
            Console.WriteLine($"----");
            Console.WriteLine($"Resultado Run: {resultadoRun}");


resultado:
            Resultado StartNew: System.Threading.Tasks.Task`1[System.Int32]
            ----
            Resultado Run: 7


-- Hacer Dispose al CancellationTokenSource  (debido a que usa timers, siempre se debe realizar el dispose cuando se trabaje con ellos)
usar Dispose o Using para ello

-- Dispose Correcto de Streams
este codigo es un problema

 private async Task MetodoAsync(){
     using( var streamWriter = new StreamWriter(stream)) {
         await streamWriter.WriteAsync("Hola Mundo");
     }
 }

Solucion1 :  usar DisposeAsync, el cual crea el bufer de manear asyncrona

 await using( var streamWriter = new StreamWriter(stream)) {
     await streamWriter.WriteAsync("Hola mundo");
 }

Solucion2 : hacer el Flush explicitamente
 using( var streamWriter = new StreamWriter(stream)) {
     await streamWriter.WriteAsync("Hola mundo");
     await streamWriter.FlushAsync();
 }


-- Paralelismo ---


           //PARALELISMO  WhenAll
            var directorioActual = AppDomain.CurrentDomain.BaseDirectory;
            var destinoBaseSecuencial = Path.Combine(directorioActual, @"Imagenes\resultado-secuencial");
            var destinoBaseParalelo = Path.Combine(directorioActual, @"Imagenes\resultado-paralelo");
            PrepararEjecucion(destinoBaseParalelo, destinoBaseSecuencial);
            Console.WriteLine("inicio ");

            var imagenes = ObtenerImagenes();

            // parte secuencial
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var imagen in imagenes)
            {
                await ProcesarImagen(destinoBaseSecuencial, imagen);
            }

            var tiempoSecuencial = stopwatch.ElapsedMilliseconds / 1000.0;
            Console.WriteLine("Secuencial - duracion en segundos: {0}", tiempoSecuencial);

            stopwatch.Restart();

            // parte paralelo

            var tareasEnumerable = imagenes.Select(async imagen => await ProcesarImagen(destinoBaseParalelo, imagen));
            await Task.WhenAll(tareasEnumerable);


            var tiempoParalelo = stopwatch.ElapsedMilliseconds / 1000.0;
            Console.WriteLine("Paralelo - duracion en segundos: {0}", tiempoParalelo);
            
            EscribirComparacion(tiempoSecuencial, tiempoParalelo);

            Console.WriteLine("fin");


        public static void EscribirComparacion(double tiempo1, double tiempo2)
        {
            var diferencia = tiempo2 - tiempo1;
            diferencia = Math.Round(diferencia, 2);
            var incrementoPorcentual = ((tiempo2 - tiempo1) / tiempo1) * 100;
            incrementoPorcentual = Math.Round(incrementoPorcentual, 2);
            Console.WriteLine($"Diferencia {diferencia} ({incrementoPorcentual}%)");
        }




            //PARALELISMO  Parallel.For

            Console.WriteLine("Secuencial ");
            for (int i = 0; i < 11; i++)
            {
                Console.WriteLine(i);
            }

            Console.WriteLine("Paralelo ");
            Parallel.For(0, 11, i => Console.WriteLine(i));

---- Velocidad
ej: multiplicacion de matrices


        public static void MultiplicarMatricesParalelo(double[,] matA, double[,] matB,
                                                double[,] result,
                                                CancellationToken token = default,
                                                int maximoGradoParalelismo = -1)
        {
            int matACols = matA.GetLength(1);
            int matBCols = matB.GetLength(1);
            int matARows = matA.GetLength(0);

            Parallel.For(0, matARows,
                new ParallelOptions()
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = maximoGradoParalelismo
                },
                i =>
                {
                    for (int j = 0; j < matBCols; j++)
                    {
                        double temp = 0;
                        for (int k = 0; k < matACols; k++)
                        {
                            temp += matA[i, k] * matB[k, j];
                        }
                        result[i, j] += temp;
                    }
                });
        }


---- Task.WhenAll vs Parallel.For
CPU vs I/O

Si es I/O, consideramos usar Task.WhenAll (ej: descarga de imagenes de internet)
Si es CPU, concideramos usar la clase Parallel.For  (ej: multiplicacion de matrices)


-- Parallel.Foreach

            var directorioActual = AppDomain.CurrentDomain.BaseDirectory;
            var carpetaOrigen = Path.Combine(directorioActual, @"Imagenes\resultado-secuencial");
            var carpetaDestinoSecuencial = Path.Combine(directorioActual, @"Imagenes\foreach-secuencial");
            var carpetaDestinoParalelo = Path.Combine(directorioActual, @"Imagenes\foreach-paralelo");
            PrepararEjecucion(carpetaDestinoSecuencial, carpetaDestinoParalelo);

            var archivos = Directory.EnumerateFiles(carpetaOrigen);


            var stopwathc = new Stopwatch();

            stopwathc.Start();
            //Secuencial
            foreach (var archivo in archivos)
            {
                VoltearImagen(archivo, carpetaDestinoSecuencial);
            }

            var tiempoSecuencial = stopwathc.ElapsedMilliseconds / 1000.0;
            Console.WriteLine("Secuencial - duración en segundos: {0}", tiempoSecuencial);

            stopwathc.Restart();
            //Paralelo
            Parallel.ForEach(archivos, archivo => {
                VoltearImagen(archivo, carpetaDestinoParalelo);
            });
            var tiempoParalelo = stopwathc.ElapsedMilliseconds / 1000.0;
            Console.WriteLine("Paralelo - duración en segundos: {0}", tiempoParalelo);

            EscribirComparacion(tiempoSecuencial, tiempoParalelo);
            Console.WriteLine("fin");


-- 
        private void VoltearImagen(string archivo, string carpetaDestino)
        {
            using (var image = new Bitmap(archivo))
            {
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                var nombreArchivo = Path.GetFileName(archivo);
                var destino = Path.Combine(carpetaDestino, nombreArchivo);
                image.Save(destino);
            }
        }

incremento de velocidad es lo mas importante para usar paralelismo
sin embargo nada es gratis. 
el paralelismo tiene su costo al preparal los hilos por ello es necesario hacer mediciones
vale la pena?
ej:

    corregir un examen = 4 mins
    encontrar 2 ayudantes = 45 mins

    corregir 1 examen
    tu solo 4*1 = 4 minutos
    en grupo: 45 + 4 = 49 minutos

    corregir 150 examenes
    tu soo: 4 * 150 = 600 min
    en grupo: 45 + 4 * 50 = 245 min


--- PARALEL INVOKE
EJECUTAR FUNCIONES DISTINTAS EN PARALELO

            // PARALEL.INVOKE
            // Preparando código de voltear imágenes
            var directorioActual = AppDomain.CurrentDomain.BaseDirectory;
            var carpetaOrigen = Path.Combine(directorioActual, @"Imagenes\resultado-secuencial");
            var carpetaDestinoSecuencial = Path.Combine(directorioActual, @"Imagenes\foreach-secuencial");
            var carpetaDestinoParalelo = Path.Combine(directorioActual, @"Imagenes\foreach-paralelo");
            PrepararEjecucion(carpetaDestinoSecuencial, carpetaDestinoParalelo);
            var archivos = Directory.EnumerateFiles(carpetaOrigen);

            // Preparando código de matrices
            int columnasMatrizA = 208;
            int filas = 1240;
            int columnasMatrizB = 750;
            var matrizA = Matrices.InicializarMatriz(filas, columnasMatrizA);
            var matrizB = Matrices.InicializarMatriz(columnasMatrizA, columnasMatrizB);
            var resultado = new double[filas, columnasMatrizB];

            Action multiplicarMatrices = () => Matrices.MultiplicarMatricesSecuencial(matrizA, matrizB, resultado);
            Action voltearImagenes = () =>
            {
                foreach (var archivo in archivos)
                {
                    VoltearImagen(archivo, carpetaDestinoSecuencial);
                }
            };

            Action[] acciones = new Action[] { multiplicarMatrices, voltearImagenes };

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // TODO: Algoritmo secuencial
            foreach (var accion in acciones)
            {
                accion();
            }

            var tiempoSecuencial = stopwatch.ElapsedMilliseconds / 1000.0;

            Console.WriteLine("Secuencial - duración en segundos: {0}",
                    tiempoSecuencial);

            PrepararEjecucion(carpetaDestinoSecuencial, carpetaDestinoParalelo);

            stopwatch.Restart();

            // TODO: Algoritmo paralelo
            Parallel.Invoke(acciones);

            var tiempoEnParalelo = stopwatch.ElapsedMilliseconds / 1000.0;

            Console.WriteLine("Paralelo - duración en segundos: {0}",
                   tiempoEnParalelo);

            EscribirComparacion(tiempoSecuencial, tiempoEnParalelo);

            Console.WriteLine("fin");

