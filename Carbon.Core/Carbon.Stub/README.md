## Carbon bootloading process
`mscorlib > Carbon.Doorstop > UnityEngine > Carbon.Stub > Carbon.Loader > (..)`

## Carbon.Stub
Is loaded by the game's built-in Harmony Loader and it's goal is to act as a stub,
we started neededing this when Facepunch created a new Harmony loader based on
Cecil which dybamically changed the assembly names of the loaded plugins. This
module will also move any other harmony plugins on the native folder to cabon's
harmony folder so we can manage them.