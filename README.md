# CPR Dummy Desktop Monitor WindowsForms
 Windows Forms application to activate and monitor CPR Dummy simulator(link to dummy assemly repository - https://github.com/dvoron89/Assembling-CPR-Dummy-2in1.git)

Hello! 
Here you will find Windows application to monitor status of CPR Dummy.

Features:
Connection:
- USB Cable. Just plug it and click "Connect via USB"
- WIFI. Dummy creates WIFI access point with certain name, starts with "CPR_Dummy_xx". Connect to this WIFI AP, using password from instructions in application. Click "Connect via WIFI"
- If, due to some reasons, connection would be lost, application will sound alarm about it and offer to reconnect
Modes:
- Training mode. In this mode dummy is "dead", objective is to reanimate it correctly. During reanimation process sound assistance will be provided. Based on results of reanimation process, when the timer is over, dummy turns "alive" or stays "dead". Results of training are never saving to database
- Exam mode. To start this mode it's obligatory to note the name of student. In general, process of examination is the same as for training except there is no suond assistance. After the timer is over, results of reanimation will be saved to database
- Archive. Database containing information about examinations. Printing results is available
