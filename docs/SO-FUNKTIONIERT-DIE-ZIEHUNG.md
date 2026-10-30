# So funktioniert die Tombaro-Ziehung

[English: How the Tombaro Draw Works](HOW-IT-WORKS.md)

Diese Seite erklärt die Tombaro Draw Engine ohne technische Vorkenntnisse.

## Was macht die Draw Engine?

Die Draw Engine hat genau eine Aufgabe:

> Sie erhält eine Liste gültiger Losnummern und zieht daraus die vorgegebene Anzahl an Gewinnern.

Ein Beispiel: Es wurden 83 Lose verkauft und nach den Regeln der Tombola müssen zwei Gewinner gezogen werden. Tombaro übergibt der Draw Engine die 83 gültigen Losnummern und die Anweisung, zwei Gewinner zu ziehen.

Die Engine liefert zwei unterschiedliche Losnummern zurück. Ein Los, das nicht in der Liste der gültigen Lose enthalten ist, kann nicht gewinnen.

## Wer bestimmt die Anzahl der Gewinner?

Nicht die Draw Engine.

Wie viele Gewinner erforderlich sind, ergibt sich aus den Regeln der jeweiligen Tombola. Diese können beispielsweise von gesetzlichen Vorgaben, dem genehmigten Spielplan oder den Festlegungen des Veranstalters abhängen.

Die Draw Engine erhält lediglich die fertige Anweisung, zum Beispiel:

> Ziehe 2 Gewinner aus diesen 83 gültigen Losen.

Diese Trennung ist beabsichtigt. Rechtliche und tombolaspezifische Regeln bleiben damit unabhängig vom eigentlichen Ziehungsalgorithmus.

## Kann eine Ziehung später überprüft werden?

Ja. Das ist eine der wichtigsten Eigenschaften des Verfahrens.

Die Engine verwendet das kryptografische Hashverfahren SHA-256, um aus den gültigen Losen eine eindeutig bestimmte Reihenfolge zu erzeugen. Die erforderliche Anzahl an Losen am Anfang dieser Reihenfolge sind die Gewinner.

Das Verfahren ist **deterministisch**. Das bedeutet: Dieselben vollständigen Ziehungsdaten führen immer wieder zum selben Ergebnis.

Werden heute beispielsweise die Losnummern 47 und 126 gezogen, erhält ein Prüfer mit exakt denselben Eingangsdaten und derselben Algorithmusversion später erneut die Losnummern 47 und 126 in derselben Reihenfolge.

## Warum ist diese Wiederholbarkeit wichtig?

Bei einer klassischen Ziehung muss darauf vertraut werden, dass die Ziehung in diesem Moment ordnungsgemäß durchgeführt wurde.

Eine reproduzierbare digitale Ziehung kann zusätzlich auch im Nachhinein überprüft werden.

Ein Verein, Prüfer, eine Behörde oder ein technisch interessierter Teilnehmer kann den veröffentlichten Algorithmus zusammen mit den ursprünglichen Ziehungsdaten verwenden und das Ergebnis unabhängig reproduzieren.

Werden relevante Ziehungsdaten verändert, verändert sich auch der kryptografische Fingerabdruck der Ziehung.

## Weiß die Draw Engine, wem ein Los gehört?

Nein.

Die Draw Engine arbeitet ausschließlich mit Losnummern. Sie benötigt weder Namen noch Adressen, Zahlungsinformationen oder Angaben darüber, wie viel eine Person ausgegeben hat.

Sie weiß auch nicht, welcher Preis zu einer bestimmten Gewinnerposition gehört. Die Zuordnung der Preise erfolgt durch die Anwendung, welche die Draw Engine verwendet.

## Kann ein nicht verkauftes oder ungültiges Los gewinnen?

Nein, sofern es nicht in der an die Engine übergebenen Liste der gültigen Lose enthalten ist.

Die Engine berücksichtigt ausschließlich die Losnummern, die ihr ausdrücklich als teilnahmeberechtigt übergeben wurden.

## Was passiert, wenn es nicht genügend gültige Lose gibt?

Soll die Engine beispielsweise zwei unterschiedliche Gewinner ziehen, es existiert aber nur ein gültiges Los, führt sie keine Ziehung durch.

Was in einem solchen Fall mit der Tombola geschieht - beispielsweise Abbruch und Rückerstattung - wird durch die Tombolaregeln und die Anwendung festgelegt, nicht durch die Draw Engine.

## Welche Informationen beeinflussen das Ergebnis?

Für die Ziehung wird eine fest definierte Menge von Daten verwendet. Dazu gehören:

- die Version des Ziehungsalgorithmus,
- die eindeutige Kennung der Tombola,
- das Ende des Losverkaufs,
- der Zufallswert für die Ziehung (technisch: Draw Nonce),
- die erforderliche Anzahl an Gewinnern und
- die vollständige Liste der gültigen Losnummern.

Diese Informationen werden in einem genau definierten Format zusammengeführt und kryptografisch verarbeitet. Daraus entsteht anschließend die Reihenfolge der gültigen Lose.

Die technische `README.md` dieses Projekts beschreibt das Verfahren im Detail.

## Was ist der Zufallswert für die Ziehung?

Der Zufallswert für die Ziehung ist ein zufällig erzeugter Wert, der in die Berechnung der Gewinner eingeht. Technisch wird er „Draw Nonce“ genannt. Wird dieser Wert verändert, kann sich auch das Ziehungsergebnis verändern.

Deshalb muss die Anwendung, welche die Draw Engine verwendet, ein klares Verfahren für die Erzeugung und Speicherung dieses Wertes festlegen. Der Zufallswert darf nicht dazu verwendet werden, eine Ziehung immer wieder mit unterschiedlichen Werten durchzuführen, bis ein gewünschtes Ergebnis entsteht.

Dieser Punkt wird in der technischen Dokumentation ausdrücklich beschrieben, damit auch andere Anwendungen, die die Open-Source-Engine einsetzen, ein geeignetes transparentes Verfahren festlegen können.

## Warum wird der Quellcode veröffentlicht?

Der Grund ist Transparenz.

Vereine und Teilnehmer sollen nicht ausschließlich darauf vertrauen müssen, dass die Ziehungssoftware korrekt funktioniert. Durch die Veröffentlichung der Draw Engine können unabhängige Entwickler, Prüfer und andere Interessierte den tatsächlich verwendeten Algorithmus einsehen.

Das Ziehungsverfahren kann damit sowohl **eingesehen** als auch **unabhängig reproduziert** werden.

## Technische Details

Die `README.md` im Hauptverzeichnis der Draw Engine beschreibt unter anderem das exakte Eingabeformat, die SHA-256-Berechnungen, die Sortierung der Lose, die Prüfregeln und alle Informationen, die zur unabhängigen Reproduktion einer Ziehung benötigt werden.


## Wie schützt Tombaro den Zufallswert vor nachträglichen Änderungen?

Bereits beim Anlegen der Tombola erzeugt Tombaro den Zufallswert und hält einen digitalen Fingerabdruck davon fest. Vor Verkaufsbeginn erhält der Verein diesen Fingerabdruck auf einem Beleg. Der Zufallswert selbst bleibt bis zur Ziehung geheim. Dieses Vorgehen wird technisch „Commit-Reveal“ genannt.

Nach Verkaufsschluss verwendet Tombaro den zuvor festgelegten Zufallswert für die Ziehung. Anschließend wird er zusammen mit den Ziehungsdaten offengelegt. Bei einer unabhängigen Prüfung kann man zuerst den Fingerabdruck des offengelegten Zufallswerts mit dem Beleg des Vereins vergleichen und danach das Ziehungsergebnis nachrechnen.

Der frühere Nachweis macht einen späteren Austausch des Zufallswerts erkennbar, wenn er mit einer unabhängig aufbewahrten Kopie verglichen wird. Voraussetzung ist, dass der Fingerabdruck vor Verkaufsbeginn erzeugt und aufbewahrt wurde. Er beweist für sich allein weder die Vollständigkeit der Losliste noch, dass jemand mit Kenntnis des Zufallswerts andere Ziehungsdaten nicht beeinflussen konnte.
