#include "rtweekend.h"
#include "hittable_list.h"
#include "material.h"
#include "sphere.h"
#include "color.h"
#include "camera.h"
#include <vector>
#include <cstring>
#include <memory>
#include <cstdint>

#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "external/stb_image_write.h"

//Makro eksportu dla C ABI
#ifndef EXPORT
#if defined(_WIN32)
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __attribute__((visibility("default")))
#endif
#endif

//Abi C-friendly API do renderowania sceny ray tracingowej z C++ do dowolnego jêzyka obs³uguj¹cego C ABI
extern "C" {

	typedef void (*RenderCallback)(int samples, uint8_t* buffer);

	// Struktura konfiguracji kamery przekazywana przez wartoœæ (C ABI-friendly)
	struct CameraConfig {
		double aspect_ratio;      
		int    image_width;        
		int    samples_per_pixel;  
		int    max_depth;         

		double vfov;               
		double lookfromX, lookfromY, lookfromZ;
		double lookatX, lookatY, lookatZ;
		double vupX, vupY, vupZ;

		double defocus_angle;      
		double focus_dist;         
	};

	EXPORT void* CreateLambertian(double r, double g, double b)
	{
		return new lambertian(color(r, g, b));
	}

	EXPORT void* CreateMetal(double r, double g, double b, double fuzz)
	{
		return new metal(color(r, g, b), fuzz);
	}

	EXPORT void* CreateDielectric(double refractionIndex)
	{
		return new dielectric(refractionIndex);
	}

	EXPORT void DestroyMaterial(void* materialPtr)
	{
		delete static_cast<material*>(materialPtr);
	}
	// SPHERE
	EXPORT void* CreateSphere(double centerX, double centerY, double centerZ, double radius, void* materialPtr) {
		auto mat = static_cast<material*>(materialPtr);
		if (!mat) return nullptr;
		// shared_ptr do materia³u z niestandardowym deleterem (materia³ zarz¹dzany zewnêtrznie)
		return new sphere(point3(centerX, centerY, centerZ), radius, std::shared_ptr<material>(mat, [](material*) {/*externally owned*/}));
	}

	EXPORT void DestroySphere(void* spherePtr) {
		if (spherePtr) {
			delete static_cast<sphere*>(spherePtr);
		}
	}

	// HITTABLE LIST (scena)
	EXPORT void* CreateScene() {
		return new hittable_list();
	}

	EXPORT void DestroyScene(void* scenePtr) {
		if (scenePtr) {
			delete static_cast<hittable_list*>(scenePtr);
		}
	}


	// Dodawanie sfery do sceny
	EXPORT bool SceneAddSphere(void* scenePtr, void* spherePtr) {
		if (!scenePtr || !spherePtr) return false;
		auto scene = static_cast<hittable_list*>(scenePtr);
		auto sp = static_cast<sphere*>(spherePtr);
		// Przekazanie w³asnoœci do shared_ptr - sfera bêdzie zwalniana przez scenê
		scene->add(std::shared_ptr<hittable>(sp));
		return true;
	}

	// RenderScene: tworzy lokalny obiekt camera i renderuje do bufora, z opcjonalnym callbackiem
	// Callback ma sygnaturê: typedef void (*RenderCallback)(int samples, uint8_t* buffer);
	EXPORT bool RenderScene(CameraConfig cfg, void* scenePtr, uint8_t* outRgbaBuffer, RenderCallback progressCallback) {
		if (!scenePtr || !outRgbaBuffer) return false;

		camera cam;
		cam.aspect_ratio = cfg.aspect_ratio;
		cam.image_width = cfg.image_width;
		cam.samples_per_pixel = cfg.samples_per_pixel;
		cam.max_depth = cfg.max_depth;

		cam.vfov = cfg.vfov;
		cam.lookfrom = point3(cfg.lookfromX, cfg.lookfromY, cfg.lookfromZ);
		cam.lookat = point3(cfg.lookatX, cfg.lookatY, cfg.lookatZ);
		cam.vup = vec3(cfg.vupX, cfg.vupY, cfg.vupZ);

		cam.defocus_angle = cfg.defocus_angle;
		cam.focus_dist = cfg.focus_dist;

		auto world = static_cast<hittable_list*>(scenePtr);

		// Wywo³anie metody render wbudowanej w kamerê
		cam.render(*world, outRgbaBuffer, progressCallback);
		return true;
	}

	// SavePng: zapisuje bufor RGBA8 do pliku PNG przez stb_image_write
	// buffer: width * height * 4 (RGBA), u³o¿enie wierszy od góry do do³u
	EXPORT bool SavePng(const char* filePath, int width, int height, const uint8_t* rgbaBuffer) {
		if (!filePath || !rgbaBuffer || width <= 0 || height <= 0) return false;
		const int stride = width * 4;
		int ok = stbi_write_png(filePath, width, height, 4, rgbaBuffer, stride);
		return ok != 0;
	}

} 