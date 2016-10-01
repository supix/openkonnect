# openkonnect
Questa applicazione scarica le timbrature dai lettori Kronotech (utilizzati anche dalla Zucchetti) utilizzando un thread per ogni lettore. Ogni thread controlla un lettore indipendentemente dagli altri. E' possibile impostare un intervallo di tempo differente per ciascun lettore, a seconda delle necessità.

Le timbrature scaricate vengono salvate in un database MySql. Nel progetto è possibile trovare lo statement DDL di creazione tabella.

I lettori vengono configurati attraverso un apposito file che ne contiene il nome, l'indirizzo IP, la porta sulla quale sono in ascolto, il tempo in secondi tra due successive interrogazioni.

Altre istruzioni a breve.
