@echo off

set JAVA_BIN=C:\Users\galax\AppData\Local\Packages\LinesOfCodes.QSM.Windows_g6c2wjg7tz59r\LocalCache\Local\QSM\Java\jdk8u422-b05\bin

%JAVA_BIN%\javac JavaCheck.java
%JAVA_BIN%\jar cvfe JavaCheck.jar JavaCheck .\JavaCheck.class
copy JavaCheck.jar ..\..\QSM.Windows\Utilities\