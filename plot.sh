#!/bin/sh
gnuplot --persist << EOF
 set datafile separator ','
 set term wxt
 plot 'kernfft.csv' using 1:2 title 'Real' with lines
EOF
