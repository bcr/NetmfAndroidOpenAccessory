What happened?
==============

I heard about the Android Open Accessory initiative and I wanted to mess with it. Turns out it needed hardware that was a) expensive and b) unobtainable (they're currently sold out). That made me sad.

Poor puppy. What did you do?
============================

I poked around on my desk, and it turns out I had a FEZ Domino with a USB host interface built in. So I implemented the host side of the protocol on it. The FEZ Domino uses the .NET Micro Framework, so it's a C# implementation.

And then what?
==============

I made a sample app using the normal Android development process, put it on my Nexus S (running normal OTA Android 2.3.4), and plugged 'em in. Now I can turn on LEDs and push buttons and everyone has a good time. They connect together and you have a bidirectional byte stream 

Really? Just normal stuff on Android?
=====================================

Yep. I just had to make the protocol look right on the FEZ. Android doesn't care what it's talking to as long as the protocol looks right.

Can I poke at it?
=================

Yep.

https://github.com/bcr/NetmfAndroidOpenAccessory is the .NET Micro Framework code.
https://github.com/bcr/HelloFez is the Android code.

There's nothing really special about the Android part. It's just a sample that turns the FEZ LED on and off and shows you the button state.

I have an action-packed video on YouTube that demonstrates what I did. http://www.youtube.com/watch?v=cHIN_Ylhk5o
