mfaktx-controller
=================
http://www.mersenneforum.org/showthread.php?t=18088 has more details.

Known issues:
If the text box has the full number of lines and you click in the text box so that it has focus, text updates cause the scroll to jump to the top of the box instead of the bottom.  Workaround: focus another element, e.g. by clicking a button.

Possible to-do list:
Use a single ini file for the basic configuration, and simply prepend or append the GPUSieveSize when forming the ini file.
(related to above) Allow user to set GPUSieveSize values from controller, either instead of slow/medium/fast or as a way to set which three values you'd like as your slow/medium/fast values
