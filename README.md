# Kingslayer

Kingslayer ist ein First-Person-Action-Shooter mit Arena-Fokus, Level-Progression und Bosskampf.

## Kurzbeschreibung

In Kingslayer kämpfst du dich aus der Ego-Perspektive durch mehrere Level, sammelst Waffen, verwaltest Munition und schaltest neue Level frei. Ammo-Boxen fuellen deine Reservemunition auf, waehrend HealPads dich im Kampfverlauf wieder heilen koennen. Das Gameplay kombiniert Fernkampf, Nahkampf und bewegungsbasiertes Ausweichen. Im letzten Abschnitt wartet ein Boss mit Flächenangriffen auf dich.

## Features

- First-Person Movement mit Springen und freiem Umsehen
- Waffensystem mit Magazin, Reserve-Munition und Reload
- Nahkampfangriff als zusätzliche Kampfoption
- Gegner-KI mit Suchen, Verfolgen, Angriff und Repositionierung
- HUD für Leben und Munition
- Ammo-Boxen zum Auffuellen von Munition
- HealPads zur Regeneration von Leben
- Levelauswahl mit Unlock-System (gesperrte Level werden ausgegraut)
- Goal-Trigger zum Abschliessen eines Levels
- GameOver- und Victory-Flow mit Panel-UI
- Bossgegner mit Warnkreis, verzögerter Explosion und Flächenschaden

## Wichtige Szenen

- MainMenu
- Level_1
- Level_2
- Level_3
- TheRange

## Kernskripte

- PlayerMovement: Bewegung, Blicksteuerung, Cursor-Handling
- PlayerShooter: Schiessen, Nachladen, Nahkampf, Trefferlogik
- PlayerHealth: Lebenssystem und Schaden
- EnemyAI: Gegnerzustandsmaschine und Angriffsverhalten
- Boss: Bossverhalten inkl. Air-Strike und Health
- LevelSelectManager: Level-Laden, Unlocks, Progress-Reset
- LevelGoal: Levelabschluss und Rückkehr in die Lobby
- PlayerGameOverController: GameOver-Panel und Tod

## Steuerung

Die Steuerung wird über das Unity Input System verwaltet.

Typisch im Projekt:

- Bewegung               WASD
- Slow-Walk             Shift
- Umsehen               Mouse
- Springen              Space
- Schiessen              MB1
- Nachladen               R
- Nahkampf                V
- Pause/Escape-Menue    Escape


## Progression und Speichern

Level-Freischaltungen werden mit PlayerPrefs gespeichert.

Verwendete Keys:

- Level2Unlocked
- Level3Unlocked

Im Main Menu gibt es eine Reset-Funktion, die diese Unlocks zurücksetzt.

## Bosskampf

Der Boss bleibt in der Luft statisch und führt periodische Bodenangriffe aus:

1. Zielpunkt wird an der aktuellen Spielerposition berechnet.
2. Ein Warnkreis erscheint am Boden.
3. Nach Verzögerung detoniert der Angriff und verursacht Flächenschaden.

Bei 0 HP wird der Boss zerstört und das Victory-Panel angezeigt.

## Voraussetzungen

- Unity 6
- Projektversion: 6000.0.65f1

## Projekt starten

1. Projekt in Unity Hub hinzufügen.
2. Mit Unity 6000.0.65f1 öffnen.
3. Szene MainMenu laden.
4. Play starten.

## Bekannte Hinweise

- Cursor-Lock/Visibility ist szenenabhängig und wird in Gameplay-Szenen erzwungen.
- Bei UI-Buttons immer sicherstellen, dass die korrekte Methode im OnClick-Event gesetzt ist.
