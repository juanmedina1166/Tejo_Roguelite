using UnityEngine;

// Enum para definir cuándo se puede activar la habilidad. ¡Puedes añadir todos los que necesites!
public enum TriggerType
{
    // Disparadores de Turno/Ronda
    OnTurnStart,            // Justo cuando empieza tu turno.
    OnRoundEnd,             // Cuando la ronda (ambos jugadores lanzan) termina.
    OnWinRound,             // Cuando ganas una ronda.
    OnLoseRound,            // Cuando pierdes una ronda.

    // Disparadores de Lanzamiento
    OnAimStart,             // Cuando empiezas a apuntar.
    OnThrow,                // Justo en el momento de lanzar el tejo.
    WhileTejoInAir,         // (Avanzado) Se activa repetidamente mientras el tejo vuela.

    // Disparadores de Puntuación
    OnScore_Mecha,          // Cuando explotas una mecha.
    OnScore_Embocinada,     // Cuando el tejo cae en el bocín.
    OnScore_Moñona,         // Cuando haces mecha y embocinada en el mismo tiro.
    OnScore_Mano,           // Cuando ganas puntos por mano (quedar más cerca).

    // Disparadores Pasivos
    Passive_AlwaysActive,   // Siempre activo, modificando estadísticas base.

    // Disparadores de Interacción con el Rival
    OnOpponentScore         // Cuando el rival anota cualquier tipo de punto.
}

[CreateAssetMenu(fileName = "NuevaHabilidad", menuName = "Roguelite/Habilidad")]
public class Habilidad : ScriptableObject
{
    [Header("Información General")]
    public string nombre;
    [TextArea(3, 5)]
    public string descripcion;
    public Sprite icono;
    public Sprite fondo;

    [Header("Lógica de la Habilidad")]
    public TriggerType trigger;

    // Aquí puedes volverte creativo. Podrías usar un enum para el tipo de efecto
    // o crear clases de efectos más complejas. Empecemos simple.
    public float valorNumerico1;
    public float valorNumerico2;
    // Podrías añadir referencias a prefabs (ej. para invocar algo), otros ScriptableObjects, etc.
}
