clear;
close all;
clc;

% -------------------------------------------------------------------------
% Import calibration data

dirName = 'Test Data/';

quaternion = {};
sensor = {};
battery = {};

listing = dir(strcat(dirName, '*_*_Quaterion.csv'));
if(length(listing) > 0)
    for i = 1:length(listing)  % for each sensor
        quaternion{i} = csvread(strcat(dirName, listing(i).name), 1, 0);
    end
end

listing = dir(strcat(dirName, '*_*_Sensor.csv'));
if(length(listing) > 0)
    for i = 1:length(listing)  % for each sensor
        sensor{i} = csvread(strcat(dirName, listing(i).name), 1, 0);
    end
end

listing = dir(strcat(dirName, '*_*_Battery.csv'));
if(length(listing) > 0)
    for i = 1:length(listing)  % for each sensor
        battery{i} = csvread(strcat(dirName, listing(i).name), 1, 0);
    end
end

% -------------------------------------------------------------------------
% Plot gyroscope x axis of all sensors

colours = hsv(9);

figure('Position',[10,40,800,450]);
hold on;
for i = 1:length(sensor)    % for each sensor
    plot(sensor{i}(:,1) / 1000, sensor{i}(:,2) / 10, 'color', colours(i,:));
end
title('Gyroscope x axis of all sensors');
xlabel('Time (s)');
ylabel('Angular velocity (degrees per second)');

labels = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I'};
legend1 = legend(labels);
set(legend1, 'Orientation', 'horizontal');
