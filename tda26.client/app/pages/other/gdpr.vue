<script setup lang="ts">
    import { Head, Title } from '#components';

    definePageMeta({
        layout: "normal-page-layout"
    });
</script>

<template>
    <Head>
        <Title>GDPR • Think different Academy</Title>
    </Head>

    <h1 :class="$style.nadpis">GDPR</h1>
    <p :class="$style.podnapis">Lorem ipsum dolor sit amet, consectetur adipisicing elit. Adipisci at commodi corporis, delectus ducimus est facilis ipsum molestiae nisi odit, reprehenderit repudiandae sapiente sit temporibus voluptates. Consectetur cumque esse itaque.</p>
</template>

<style module lang="scss">
.nadpis {
    font-size: 64px;
    margin:0;
}

.podnapis {
    font-size: 20px;
    margin-top: 16px;
    max-width: 700px;
    color: var(--text-color-secondary);
}
</style>