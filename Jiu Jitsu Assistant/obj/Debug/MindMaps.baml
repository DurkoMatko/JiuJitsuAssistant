   M S B A M L   `   `   `  ����O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  5   	   N JJiu Jitsu Assistant, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:Jiu_Jitsu_AssistantJiu_Jitsu_Assistant 5   	   "   Jiu_Jitsu_Assistant.MindMaps   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   6sys&clr-namespace:System;assembly=mscorlib     5   	   3local!clr-namespace:Jiu_Jitsu_Assistant     5   	   Z VWPFToolkit.Extended, Version=1.6.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4:
extToolkit)http://schemas.xceed.com/wpf/xaml/toolkit  5   	   -   5        $��mindmaps_window��5   	      MindMaps_Title  � Title#  �   5
   	    � 
ResizeMode$ NoResize=�5   	    � Icon$ /images/bjj-logo.png��5   	   #��� ��5   	   $��800px��5   	   $��1300px��6   .��5      �  � ColumnDefinitions 5      �� 5      $	��400 �6   �� 5      $��* �6   �� 5      -   5         �Name$ gameplayColumn��5   (   $	��900 �6   6(   .+�5      �� -   5         ��Name$ label1��5        Settings_Label_Playboard#���  6   $
��Left=�6S   $��	42,20,0,0q�6n   $	��Top=�6�   $��0��6�   $��69��6�   
���   �$��16(�6�   �� 5      -   5         ��Name$ CanvasBorder��5      
���   �$��3q�69   $��38,45,36,507q�6M   ._�5      �� $
��Left=�6   $��24,5,0,0q�6-   $	��270��6?   $��0��6K   $	��198��6[   $	��Top=�6h   .+�5      �� -   5         ��Name$ newRoundButton��5        NewRoundButton_Playboard#���  6-     StyleButton#��[ 6b    �� Left$	 158��6�   
	 �� Top$		 130��6�   $	��102��6�   $��40��6�   $��14(�6�   6�   �� 5      -   5        $ resetButton��5        ResetButton_Playboard#���  6*   #��[ 6\   $ 10��6�   $		 130��6�   $	��102��6�   $��40��6�   $��14(�6�   6�   �� 5      -   5        
 ��Name$
 difficultyCombobox��5      $ 10��63   $	 21��6D   $��Center=�6U   $��Center=�6w   $	��250��6�   $��36��6�   .@�5      �� $
��Easy��6    �� 
IsSelected . 
 � Tag 5      X�5      0.40Q�6(   5!      �� 5"      $��Medium��6    5#      X�5$      0.50Q�6(   5&      �� 5'      $
��Hard��6    5(      X�5)      0.60Q�6(   5+      5,      	� 5-      -   5         	�Name$ gi_radioButton��5-      $��Gi��62   $ 10��6?   $	 84��6P   6`   	� 5.      -	   5        $ nogi_radioButton��5.      $
��NoGi��64   $ 69��6C   $	 84��6T   6d   5/      50   	   (  "Xceed.Wpf.Toolkit.WatermarkTextBox  52      -
   5          Name$ setup_textBox��52   $     SetupWatermark_Playboard   	Watermark# �  6;   $
��Left=�6s   $��80��6�   $��
42,646,0,0q�6�    �� TextWrapping$
 Wrap=�6�   $	��Top=�6�   $	��322��6�   $��0��6�   6�   �� 53      -   5        $ label��53        CurrPosLabel_Playboard#���  6   $
��Left=�6P   $��
42,533,0,0q�6k   $	��Top=�6   $��14(�6�   
���   �$��0��6�   �� 54      -   5        $ currentPositionLabel��54      $
��none��6,   $
��Left=�6;   $��155,529,0,0q�6V   $	��Top=�6k   $��18(�6�   
��� � � J� RenderTransformOrigin$ 0.28,-0.235&�6�   �� 55      -   5        $ 
label_Copy��55        LastTechLabel_Playboard#���  6"   $
��Left=�6V   $��
42,568,0,0q�6q   $	��Top=�6�   $��14(�6�   
���   �$��0��6�   �� 56      $	��214��6   $
��Left=�6   
��� � �$��18(�6H   $��150,568,0,125q�6�    �� Target 6V   ��  �� ElementName$ lastTechniqueLabel��.��57      �� -   5        $��lastTechniqueLabel��57      $��-��60    �� TextWrapping$ WrapWithOverflow=�69   $	��209��6Y   $��64��6e   .��6q   58      59   	   �� 5<      $��1��6   $��Stretch=�6%   $��Center=�6A   $��8��6^   ��5=      �� 5>       �� 
TargetType" ��6   .R�5?      � .+�5@      �� $	��⁞��6   �� 5A      ���#0000006   5B      5C      5E   	   �� 5I      $��2��6   $
��1000��6"   ��5J      �� 5K       �� Color$ Black��6   5M   	   �� 5N      $��2��6   $��358,41,344,553q�6"   �� 6;   .F�5O      �� $��images/bjj-logo.png��6   $	��173��62   $	��198��6?   5P      6    A� 5Q      -   5         A�Name$ buttonsGrid��5Q      $��2��6)   5S   	   �� 5T      -   5        $ label2��5T      
���   �  SequenceLabel_Playboard#���  61   $��14(�6e   $
��Left=�6s   $��
42,335,0,0q�6�   $	��Top=�6�   �� 5U      -   5         ��Name$ sequence_textblock��5U      $
��Left=�6.   $��127,342,0,0q�6I   $
��Wrap=�6^   $	��Top=�6r   $	��130��6�   $	��237��6�   5W      5X      