mfaktx-controller
=================
http://www.mersenneforum.org/showthread.php?t=18088 has more details.

Known issues:<br />
If the text box has the full number of lines and you click in the text box so that it has focus, text updates cause the scroll to jump to the top of the box instead of the bottom.  Workaround: focus another element, e.g. by clicking a button.
<br />
If you use have idle detection enabled and have both a screen saver and the power saving display off feature enabled, when the display goes off, the speed switches from Fast to Medium.  Workaround: disable power saving feature.

Possible to-do list:
<br />
Use a single ini file for the basic configuration, and simply prepend or append the GPUSieveSize when forming the ini file.
<br />
(related to above) Allow user to set GPUSieveSize values from controller, either instead of slow/medium/fast or as a way to set which three values you'd like as your slow/medium/fast values
