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






































